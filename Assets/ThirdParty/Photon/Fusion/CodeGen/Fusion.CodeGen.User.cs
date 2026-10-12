#if FUSION_WEAVER && FUSION_WEAVER_ILPOSTPROCESSOR
namespace Fusion.CodeGen {
  partial class ILWeaverSettings {
    static partial void OverrideNetworkProjectConfigPath(ref string path) {
      path = "Assets/ThirdParty/Photon/Fusion/Resources/NetworkProjectConfig.fusion";
    }
  }
}
#endif
