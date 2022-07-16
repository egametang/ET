using Microsoft.CodeAnalysis;

namespace ET.Analyzer
{
    public static class ETTaskInSyncMethodAnalyzerRule
    {
        private const string Title = "ETTask方法调用在非异步方法体内使用错误";

        private const string MessageFormat = "方法: {0} 在非异步方法体内使用时需要添加.Coroutine()后缀";

        private const string Description = "ETTask方法调用在非异步方法体内使用错误.";
            
        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.ETTaskInSyncMethodAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }
    
    public static class ETTaskInAsyncMethodAnalyzerRule
    {
        private const string Title = "ETTask方法调用在异步方法体内使用错误";

        private const string MessageFormat = "方法: {0} 在异步方法体内使用时需要添加await前缀 或 .Coroutine()后缀";

        private const string Description = "ETTask方法调用在异步方法体内使用错误.";
            
        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.ETTaskInAsyncMethodAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class UniqueIdRangeAnaluzerRule
    {
        private const string Title = "唯一Id字段数值区间约束";

        private const string MessageFormat = "类: {0} Id字段: {1}的值： {2} 不在约束的区间内, 请修改";

        private const string Description = "唯一Id字段数值区间约束.";
        
        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.UniqueIdRangeAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Model,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class UniqueIdDuplicateAnalyzerRule
    {
        private const string Title = "唯一Id字段禁止重复";

        private const string MessageFormat = "类: {0} Id字段: {1}的值： {2} 与其他Id字段值重复, 请修改";

        private const string Description = "唯一Id字段禁止重复.";
        
        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.UniqueIdDuplicateAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Model,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }
}