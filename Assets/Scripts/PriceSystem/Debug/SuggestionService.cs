
namespace PriceSystem.Debug
{
    public class SuggestionService
    {
        // readonly ResourcesManager _resources;
        //
        // // 注入！不要用 static Inst
        // public SuggestionService(ResourcesManager resources): base(waitAsync: ture)
        // {
        //     _resources = resources;
        // }
        //
        // /// <summary>
        // /// 获取 Action 层级的联想建议（第一token，不含斜杠）
        // /// </summary>
        // /// <param name="prefix">当前输入的前缀字符/param>
        // /// <returns>匹配该前缀的所action 名称</returns>
        // public List<string> GetActionSuggestions(string prefix)
        // {
        //     return _resources.GetConfig<CommandConfig>().GetAllCouponDataList()
        //         .Select(commandData => commandData.action) // 提取所action 名称
        //         .Where(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) // 匹配前缀
        //         .Distinct(StringComparer.OrdinalIgnoreCase)
        //         .ToList();
        // }
        //
        // /// <summary>
        // /// 获取 Module 层级的联想建议（第二token
        // /// </summary>
        // /// <param name="action">用户已输入的 action</param>
        // /// <param name="prefix">当前输入module 前缀</param>
        // /// <returns>匹配action 下的所module 名称</returns>
        // public List<string> GetModuleSuggestions(string action, string prefix)
        // {
        //     return _resources.GetConfig<CommandConfig>().GetAllCouponDataList()
        //         .Where(commandData => commandData.action.Equals(action, StringComparison.OrdinalIgnoreCase)) // 筛action
        //         .Select(commandData => commandData.module) // 提取 module 名称
        //         .Where(m => m.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) // 匹配前缀
        //         .Distinct(StringComparer.OrdinalIgnoreCase)
        //         .ToList();
        // }
        //
        // /// <summary>
        // /// 获取参数层级的联想建议（第三及后token
        // /// </summary>
        // /// <param name="action">用户输入action</param>
        // /// <param name="module">用户输入module</param>
        // /// <param name="paramIndex">参数索引（从0开始）</param>
        // /// <param name="prefix">参数当前输入的前缀</param>
        // /// <returns>匹配该参数位置可选值的建议列表</returns>
        // public List<string> GetParameterSuggestions(string action, string module, int paramIndex, string prefix)
        // {
        //     // 查找命令
        //     var cmd = _resources.GetConfig<CommandConfig>().GetAllCouponDataList()
        //         .FirstOrDefault(cd => cd.action.Equals(action, StringComparison.OrdinalIgnoreCase) &&
        //                               cd.module.Equals(module, StringComparison.OrdinalIgnoreCase));
        //     if (cmd.action == null || paramIndex < 0 || paramIndex >= cmd.parameters.Count)
        //         return new List<string>();
        //
        //     var providerKey = cmd.parameters[paramIndex].suggestionProvider;
        //     if (string.IsNullOrEmpty(providerKey))
        //         return new List<string>();
        //
        //     List<string> raw;
        //     switch (providerKey)
        //     {
        //         case "ItemList":
        //             raw = _resources
        //                 .GetConfig<ItemConfig>()
        //                 .GetAllItems()
        //                 .Select(i => i.itemID)
        //                 .ToList();
        //             break;
        //         case "CouponList":
        //             raw = _resources
        //                 .GetConfig<CouponConfig>()
        //                 .GetAllCouponDataList()
        //                 .Select(coupon => coupon.couponID)
        //                 .ToList();
        //             break;
        //         case "TarotList":
        //             raw = _resources
        //                 .GetConfig<TarotConfig>()
        //                 .GetAllTarotDataList()
        //                 .Select(tarot => tarot.id)
        //                 .ToList();
        //             break;
        //         case "FirstDieValue":
        //         case "SecondDieValue":
        //         case "ThirdDieValue":
        //         case "FourthDieValue":
        //         case "FifthDieValue":
        //         case "SixthDieValue":
        //             raw = new List<string> {"1", "2", "3", "4", "5", "6"};
        //             break;
        //         case "DiceIndex":
        //         case "DieIndex":
        //             raw = new List<string> {"0", "1", "2", "3", "4", "5"};
        //             break;
        //         case "Amount":
        //             raw = new List<string> {"100", "200", "500", "1000"};
        //             break;
        //         case "MaterialList":
        //             raw = _resources
        //                 .GetConfig<DiceMaterialForgeConfig>()
        //                 .allMat
        //                 .Select(m => m.id)
        //                 .ToList();
        //             break;
        //         case "AbilityList":
        //             raw = _resources
        //                 .GetConfig<DiceFaceForgeConfig>()
        //                 .RandomableFace
        //                 .Select(f => f.id)
        //                 .ToList();
        //             break;
        //         case "DiceFaceList":
        //             raw = _resources
        //                 .GetConfig<ExpansionPackConfig>()
        //                 .GetAllExpansionPacks().Where(ex => ex.expansionPackType == ExpansionPackType.ModifyDiceFace)
        //                 .Select(ex => ex.expansionPackID)
        //                 .ToList();
        //             break;
        //         case "DiceMaterialList":
        //             raw = _resources
        //                 .GetConfig<ExpansionPackConfig>()
        //                 .GetAllExpansionPacks().Where(ex => ex.expansionPackType == ExpansionPackType.ModifyDiceFace)
        //                 .Select(ex => ex.expansionPackID)
        //                 .ToList();
        //             break;
        //         case "ComboList":
        //             raw = _resources
        //                 .GetConfig<ExpansionPackConfig>()
        //                 .GetAllExpansionPacks().Where(ex => ex.expansionPackType == ExpansionPackType.ComboEnhancement)
        //                 .Select(ex => ex.expansionPackID)
        //                 .ToList();
        //             break;
        //         default:
        //             raw = new List<string>();
        //             break;
        //     }
        //
        //     // 前缀过滤
        //     return raw.Where(s => s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        // }
    }
}