using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    public struct CreateVo
    {
        /// <summary>
        /// 只能是.txt 的模板
        /// 重写文件不需要模板
        /// </summary>
        public string TemplatePath;

        /// <summary>
        /// 文件保存的位置
        /// </summary>
        public string SavePath;

        public CreateVo(string templatePath, string savePath)
        {
            TemplatePath = templatePath;
            SavePath     = savePath;
        }
    }

    public abstract class BaseTemplate
    {
        /// <summary>
        /// 这个模板动作的名称
        /// 作用于提示信息
        /// </summary>
        public virtual string EventName => "";

        protected CreateVo CreateVo;

        //常规新文件用字典
        protected Dictionary<string, string> ValueDic;

        public BaseTemplate(string authorName)
        {
            var dt = DateTime.Now;
            ValueDic               = new Dictionary<string, string>();
            ValueDic["Author"]     = authorName;
            ValueDic["CreateDate"] = $"{dt.Year}.{dt.Month}.{dt.Day}";
        }

        /// <summary>
        /// 是否覆盖  如果存在的情况下
        /// 写入新文件时可用
        /// </summary>
        public virtual bool Cover => true;

        /// <summary>
        /// 其他是否保留
        /// 重写文件时可用
        /// </summary>
        public virtual bool OtherRetain => true;

        /// <summary>
        /// 如果值已存在是否跳过
        /// 重写文件时可用
        /// </summary>
        public virtual bool ExistSkip => true;

        /// <summary>
        /// 完成后自动刷新
        /// </summary>
        public virtual bool AutoRefresh => true;

        /// <summary>
        /// 提示
        /// </summary>
        public virtual bool ShowTips => true;

        /// <summary>
        /// 根据模板创建一个新文件
        /// </summary>
        /// <returns></returns>
        protected bool CreateNewFile()
        {
            if (TemplateEngine.FileExists(CreateVo.SavePath))
            {
                if (!Cover)
                {
                    if (ShowTips)
                    {
                        var code = $"文件已存在 当前选择已存在不覆盖 {CreateVo.SavePath}";
                        UnityTipsHelper.ShowWarning(code);
                    }

                    return false;
                }
            }

            if (!TemplateEngine.CreateCodeFile(CreateVo.SavePath, CreateVo.TemplatePath, ValueDic))
            {
                UnityTipsHelper.Show(CreateVo.SavePath + "创建失败");
                TemplateEngine.TemplateBasePath = null;
                return false;
            }

            if (ShowTips)
            {
                UnityTipsHelper.Show($"{EventName} 处理完毕");
            }

            if (AutoRefresh)
            {
                AssetDatabase.Refresh();
            }

            return true;
        }

        /// <summary>
        /// 重写文件
        /// 指定区域内的都会被重写 不存在检查 是否重复的情况
        /// 这种使用一般是这块区域就是固定的区域不允许玩家乱写的
        /// </summary>
        /// <returns></returns>
        protected bool OverrideCodeFile()
        {
            if (!TemplateEngine.FileExists(CreateVo.SavePath))
            {
                if (ShowTips)
                {
                    var code = $"文件不存在 无法重写 {CreateVo.SavePath}";
                    UnityTipsHelper.ShowError(code);
                }

                return false;
            }

            if (!TemplateEngine.OverrideCodeFile(CreateVo.SavePath, ValueDic, OtherRetain))
            {
                UnityTipsHelper.ShowError(CreateVo.SavePath + "创建失败");
                TemplateEngine.TemplateBasePath = null;
                return false;
            }

            if (ShowTips)
            {
                UnityTipsHelper.Show($"{EventName} 处理完毕");
            }

            if (AutoRefresh)
            {
                AssetDatabase.Refresh();
            }

            return true;
        }

        /// <summary>
        /// 区域内重写文件
        /// 但是会检查  如果我要写的东西已经存在了就不写了
        /// 如: 我要提供一个方法给玩家 如果这个方法已经存在了就不检查了 否则我就会创建一个新的方法
        /// 使用此方法需要吧上面的 OverrideDic 赋值
        /// </summary>
        /// <param name="overrideDic">数据</param>
        /// <param name="cover">是否覆盖 则不需要检查是否有直接全部覆盖</param>
        /// <returns></returns>
        protected bool OverrideCheckCodeFile(Dictionary<string, List<Dictionary<string, string>>> overrideDic, bool cover = false)
        {
            if (!TemplateEngine.FileExists(CreateVo.SavePath))
            {
                if (ShowTips)
                {
                    var code = $"文件不存在 无法重写 {CreateVo.SavePath}";
                    UnityTipsHelper.ShowError(code);
                }

                return false;
            }

            if (overrideDic == null)
            {
                Debug.LogError("替换数据无值 无法操作 OverrideDic == null");
                return false;
            }

            if (overrideDic.Count >= 1)
            {
                if (!TemplateEngine.OverrideCheckCodeFile(CreateVo.SavePath, overrideDic, cover))
                {
                    UnityTipsHelper.ShowError(CreateVo.SavePath + "创建失败");
                    TemplateEngine.TemplateBasePath = null;
                    return false;
                }
            }

            if (ShowTips)
            {
                UnityTipsHelper.Show($"{EventName} 处理完毕");
            }

            if (AutoRefresh)
            {
                AssetDatabase.Refresh();
            }

            return true;
        }
    }

    /// <summary>
    /// 这是一个案例 只需要new这个类 即可进行一系列模板操作
    /// 创建一个新文件时
    /// </summary>
    public class TestTemplate : BaseTemplate
    {
        public override string EventName => "测试案例一 创建新文件";
        public override bool   Cover     => false;

        public TestTemplate(string authorName, string moduleName, string pkgName, string resName) : base(authorName)
        {
            CreateVo = new CreateVo("Assets/.../Template/BasePanelTemplate.txt",
                $"Assets/../{moduleName}/UI/{resName}.cs");

            ValueDic["moduleName"] = moduleName;
            ValueDic["uiPkgName"]  = pkgName;
            ValueDic["uiResName"]  = resName;

            CreateNewFile();
        }
    }

    /// <summary>
    /// 这是一个案例 只需要new这个类 即可进行一系列模板操作
    /// 重写文件内容
    /// </summary>
    public class TestTemplate2 : BaseTemplate
    {
        public override string EventName => "测试案例二 重写文件内容";

        //public override bool OtherRetain => false;

        public TestTemplate2(string authorName, string moduleName, string pkgName, string resName) : base(authorName)
        {
            CreateVo = new CreateVo("Assets/.../Template/BasePanelTemplate.txt",
                $"Assets/../{moduleName}/UI/{resName}.cs");

            ValueDic["moduleName"] = moduleName;
            ValueDic["uiPkgName"]  = pkgName;
            ValueDic["uiResName"]  = resName;
            ValueDic["AA"]         = "//我替换的东西"; //的相当于 标记为AA 这个范围内的所有东西都会被清空 替换我指定的内容

            OverrideCodeFile();
        }
    }
}