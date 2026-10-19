# Refactoring Technique Selection

Use this local selection aid when choosing transformations. It is not an exhaustive
catalog or a requirement to add a pattern. Choose by the demonstrated problem and
preserve the agreed behavior; project architecture and compatibility rules still apply.

## Methods and Expressions

- **Extract Method:** give a cohesive operation its own name and boundary.
- **Inline Method:** remove indirection when the body communicates the intent better.
- **Extract Variable:** name a meaningful intermediate value in a complex expression.
- **Replace Temp with Query:** expose a repeatable computation through a focused query;
  account for evaluation cost and side effects.
- **Substitute Algorithm:** replace a mechanism while preserving required outcomes.

## Responsibility and Dependencies

- **Move Method / Move Field:** place behavior or state with its appropriate owner.
- **Extract Class:** separate a coherent responsibility from an overloaded owner.
- **Inline Class:** absorb a type that adds no useful independent responsibility.
- **Hide Delegate:** hide a collaborator traversal behind the appropriate owner.
- **Remove Middle Man:** eliminate forwarding that provides no useful boundary.

## Data and Interfaces

- **Replace Data Value with Object:** give a domain value its own meaning and rules.
- **Encapsulate Field / Encapsulate Collection:** control access and mutation.
- **Replace Magic Number with Symbolic Constant:** name a stable semantic value.
- **Introduce Parameter Object:** group values that form a cohesive input concept.
- **Preserve Whole Object:** pass an appropriate cohesive input instead of fragments;
  avoid coupling the receiver to an unnecessarily broad object.
- **Rename Method / Remove Parameter:** clarify intent or remove an unnecessary input.
- **Separate Query from Modifier:** separate observation from state changes.

## Conditions and Generalization

- **Decompose Conditional:** name the condition and meaningful branches.
- **Consolidate Conditional Expression:** combine conditions with the same meaning/result.
- **Replace Nested Conditional with Guard Clauses:** expose exceptional paths early.
- **Replace Conditional with Polymorphism:** place variant behavior behind a common
  contract when the variants justify separate implementations.
- **Extract Interface:** expose the capability consumers actually need.
- **Pull Up Method / Push Down Method:** place inherited behavior at its valid level.
- **Collapse Hierarchy:** remove inheritance levels without a useful distinction.
- **Replace Inheritance with Delegation:** separate reuse/collaboration from subtype identity.

## Techniques and Patterns

Techniques describe transformations; patterns describe a resulting collaboration.
For example, moving interchangeable algorithms behind a contract may use Strategy;
extracting a class alone does not establish that pattern. Explain actual roles and
benefit when a pattern applies. Use another established technique or a plainly
described custom transformation when these examples do not fit.
