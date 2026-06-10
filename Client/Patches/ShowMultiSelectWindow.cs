using System.Reflection;
using SPT.Reflection.Patching;

namespace _OpenBarters.Patches;
public class ShowMultiSelectWindow : ModulePatch
{
    protected override MethodBase GetTargetMethod() {
        return typeof(RepairControllerClass).GetMethod("method_1", BindingFlags.Instance | BindingFlags.Public);
    }
}