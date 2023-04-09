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

    public static class AddChildTypeAnalyzerRule
    {
        private const string Title = "AddChild方法类型约束错误";

        private const string MessageFormat = "Type: {0} 不允许作为实体: {1} 的AddChild函数参数类型! 若要允许该类型作为参数,请使用ChildOfAttribute对child实体类标记父级类型";

        private const string Description = "AddChild方法类型约束错误.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.AddChildTypeAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class DisableAccessEntityChildAnalyzerRule
    {
        private const string Title = "禁止在Entity类中直接调用Child和Component";

        private const string MessageFormat = "禁止在Entity类中直接调用Child和Component";

        private const string Description = "禁止在Entity类中直接调用Child和Component.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.DisableUseChildComponentInEntityAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class EntityComponentAnalyzerRule
    {
        private const string Title = "实体类添加或获取组件类型错误";

        private const string MessageFormat = "组件类型: {0} 不允许作为实体: {1} 的组件类型! 若要允许该类型作为参数,请使用ComponentOfAttribute对组件类标记父级实体类型";

        private const string Description = "实体类添加或获取组件类型错误.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.EntityComponentAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class StaticFieldDeclarationAnalyzerRule
    {
        private const string Title = "Static字段声明需要标记标签";

        private const string MessageFormat = "Static字段声明 {0} 需要标记标签";

        private const string Description = "Static字段声明需要标记标签.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.StaticFieldDeclarationAnalyzerRule,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.All, 
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }
    
    public static class CheckETCancellTokenAfterAwaitAnalyzerRule
    {
        private const string Title = "含有ETCancelToken参数的异步函数内调用await表达式后必须判断CancelToken.IsCancel";

        private const string MessageFormat = "含有ETCancelToken参数的异步函数内调用await表达式后必须判断CancelToken.IsCancel";

        private const string Description = "含有ETCancelToken参数的异步函数内调用await表达式后必须判断CancelToken.IsCancel.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.ETCancellationTokenAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.All, 
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }
    
    public static class AwaitExpressionCancelTokenParamAnalyzerRule
    {
        private const string Title = "含有ETCancelToken参数的异步函数内调用await表达式必须传入同一个CancelToken";
    
        private const string MessageFormat = "含有ETCancelToken参数的异步函数内调用await表达式必须传入同一个CancelToken";
    
        private const string Description = "含有ETCancelToken参数的异步函数内调用await表达式必须传入同一个CancelToken.";
    
        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.AwaitExpressionCancelTokenParamAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.All, 
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }
    
    public static class AsyncMethodWithCancelTokenParamAnalyzerRule
    {
        private const string Title = "异步函数声明处的ETCancelToken参数禁止声明默认值";
    
        private const string MessageFormat = "异步函数声明处的ETCancelToken参数禁止声明默认值";
    
        private const string Description = "异步函数声明处的ETCancelToken参数禁止声明默认值.";
    
        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.AsyncMethodWithCancelTokenParamAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.All,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }
    
    
    public static class ExpressionWithCancelTokenParamAnalyzerRule
    {
        private const string Title = "函数调用处的ETCancelToken参数禁止传入null";
    
        private const string MessageFormat = "函数调用处的ETCancelToken参数禁止传入null";
    
        private const string Description = "函数调用处的ETCancelToken参数禁止传入null.";
    
        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.ExpressionWithCancelTokenParamAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.All,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class EntityClassDeclarationAnalyzerRule
    {
        private const string Title = "实体类限制多层继承";

        private const string MessageFormat = "类: {0} 不能继承Entiy的子类 请直接继承Entity";

        private const string Description = "实体类限制多层继承.";

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticIds.EntityClassDeclarationAnalyzerRuleId,
            Title,
            MessageFormat,
            DiagnosticCategories.All,
            DiagnosticSeverity.Error, true, Description);
    }

    public static class EntityDelegateDeclarationAnalyzerRule
    {
        private const string Title = "实体类禁止声明委托字段或属性";

        private const string MessageFormat = "实体类: {0} 不能在类内部声明委托字段或属性: {1}";

        private const string Description = "实体类禁止声明委托字段或属性.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.DelegateAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Model,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class EntityFieldDeclarationInEntityAnalyzerRule
    {
        private const string Title = "实体类禁止声明实体字段";

        private const string MessageFormat = "实体类: {0} 不能在类内部声明实体字段: {1}";

        private const string Description = "实体类禁止声明实体字段.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.EntityFieldDeclarationInEntityAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Model,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class AsyncMethodReturnTypeAnalyzerRule
    {
        private const string Title = "禁止声明返回值为void的异步方法";

        private const string MessageFormat = "禁止声明返回值为void的异步方法";

        private const string Description = "禁止声明返回值为void的异步方法.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.AsyncMethodReturnTypeAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Model,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }

    public static class ClientClassInServerAnalyzerRule
    {
        private const string Title = "禁止在Server程序集内引用ET.Client命名空间";

        private const string MessageFormat = "禁止在Server程序集内引用ET.Client命名空间";

        private const string Description = "禁止在Server程序集内引用ET.Client命名空间.";

        public static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.ClientClassInServerAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Model,
                    DiagnosticSeverity.Error,
                    true,
                    Description);
    }
}