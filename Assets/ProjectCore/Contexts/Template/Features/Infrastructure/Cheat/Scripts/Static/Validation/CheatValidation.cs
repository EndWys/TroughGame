using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;

namespace ProjectCore.Template
{
    public static class CheatValidation
    {
        public static bool IsValidCommandName(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return false;

            foreach (char character in commandName)
            {
                if (!char.IsLetterOrDigit(character)
                    && character != '_'
                    && character != '-'
                    && character != '.')
                {
                    return false;
                }
            }

            return true;
        }

        public static Result ValidateMethod(MethodInfo method, CheatCommandAttribute attribute)
        {
            if (!IsValidCommandName(attribute.Name))
                return Result.Failure(CheatErrors.InvalidHandler(
                    method.DeclaringType, $"invalid command name '{attribute.Name}'."));
            if (method.IsGenericMethodDefinition || method.ContainsGenericParameters)
                return Result.Failure(CheatErrors.InvalidHandler(
                    method.DeclaringType, $"method '{method.Name}' is generic."));
            if (method.ReturnType == typeof(void)
                && method.GetCustomAttribute<AsyncStateMachineAttribute>() != null)
            {
                return Result.Failure(CheatErrors.InvalidHandler(
                    method.DeclaringType, $"method '{method.Name}' is async void."));
            }
            if (method.ReturnType == typeof(UniTaskVoid))
                return Result.Failure(CheatErrors.InvalidHandler(
                    method.DeclaringType, $"method '{method.Name}' returns UniTaskVoid."));
            if (typeof(System.Threading.Tasks.Task).IsAssignableFrom(method.ReturnType))
                return Result.Failure(CheatErrors.InvalidHandler(
                    method.DeclaringType, $"method '{method.Name}' must return UniTask instead of Task."));
            if (method.ReturnType == typeof(System.Threading.Tasks.ValueTask)
                || method.ReturnType.IsGenericType
                && method.ReturnType.GetGenericTypeDefinition() == typeof(System.Threading.Tasks.ValueTask<>))
            {
                return Result.Failure(CheatErrors.InvalidHandler(
                    method.DeclaringType,
                    $"method '{method.Name}' must return UniTask instead of ValueTask."));
            }

            int cancellationTokenCount = 0;
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (parameter.ParameterType == typeof(CancellationToken))
                {
                    cancellationTokenCount++;
                    continue;
                }

                if (parameter.ParameterType.IsByRef
                    || parameter.ParameterType.IsPointer
                    || parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
                {
                    return Result.Failure(CheatErrors.InvalidHandler(
                        method.DeclaringType,
                        $"parameter '{parameter.Name}' in method '{method.Name}' " +
                        "uses an unsupported signature."));
                }
            }

            if (cancellationTokenCount > 1)
            {
                return Result.Failure(CheatErrors.InvalidHandler(
                    method.DeclaringType,
                    $"method '{method.Name}' has more than one CancellationToken."));
            }

            return Result.Success();
        }
    }
}
