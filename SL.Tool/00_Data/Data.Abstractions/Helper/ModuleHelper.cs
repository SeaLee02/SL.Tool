using Data.Abstractions.Entities;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abstractions.Helper
{
    public static class ModuleHelper
    {
        /// <summary>
        /// 公共创建文件方法
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="savePath"></param>
        /// <param name="fileName"></param>
        /// <param name="model"></param>
        public static void CreateFileCommon(string templatePath, string savePath, string fileName, object model)
        {
            string guid = Guid.NewGuid().ToString();
            string text = FileHelper.FileRead(templatePath);

            var result = Engine.Razor.RunCompile(templateSource: text, name: guid, model: model);
            if (!FileHelper.IsExistFile(savePath + $@"\{fileName}"))
            {
                if (!FileHelper.IsExistDirectory(savePath))
                {
                    FileHelper.CreateDirectory(savePath);
                }
                FileHelper.CreateFile(savePath + $@"\{fileName}", result);
            }
        }


        /// <summary>
        /// 创建DirectoryBuild 文件
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="savePath"></param>
        /// <param name="modulePath"></param>
        public static void CreateDirectoryBuild(string templatePath, string savePath, ModuleModel moduleModel)
        {
            string guid = Guid.NewGuid().ToString();
            string text = FileHelper.FileRead(templatePath);

            string _id = "10" + moduleModel.Id;
            var model = new
            {
                Id = _id,
                Name = moduleModel.Name,
                PrefixSpace = moduleModel.PrefixSpace
            };
            var result = Engine.Razor.RunCompile(templateSource: text, name: guid, model: model);
            string path = savePath + $@"\{moduleModel.ModulePath}";
            string namePath = path + @$"\Directory.Build.props";
            if (!FileHelper.IsExistFile(namePath))
            {
                if (!FileHelper.IsExistDirectory(path))
                {
                    FileHelper.CreateDirectory(path);
                }
                FileHelper.CreateFile(namePath, result);
            }
        }


        /// <summary>
        /// 创建Core项目
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="savePath"></param>
        /// <param name="modulePath"></param>
        public static void CreateCodeModule(string templatePath, string savePath, ModuleModel moduleModel)
        {
            string guid = Guid.NewGuid().ToString();
            string text = FileHelper.FileRead(templatePath);
            var result = Engine.Razor.RunCompile(templateSource: text, name: guid);

            string path = savePath + $@"\{moduleModel.ModulePath}\{moduleModel.Name}.Core";
            string namePath = path + @$"\{ moduleModel.Name}.Core.csproj";
            if (!FileHelper.IsExistFile(namePath))
            {
                if (!FileHelper.IsExistDirectory(path))
                {
                    FileHelper.CreateDirectory(path);
                }
                FileHelper.CreateFile(namePath, result);
            }
        }

        /// <summary>
        /// 创建Core项目
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="savePath"></param>
        /// <param name="modulePath"></param>
        public static void CreateWebModule(string templatePath, string savePath, ModuleModel moduleModel)
        {
            string guid = Guid.NewGuid().ToString();
            string text = FileHelper.FileRead(templatePath);
            var result = Engine.Razor.RunCompile(templateSource: text, name: guid, model: moduleModel);

            string path = savePath + $@"\{moduleModel.ModulePath}\{moduleModel.Name}.Web";
            string namePath = path + @$"\{ moduleModel.Name}.Web.csproj";
            if (!FileHelper.IsExistFile(namePath))
            {
                if (!FileHelper.IsExistDirectory(path))
                {
                    FileHelper.CreateDirectory(path);
                }
                FileHelper.CreateFile(namePath, result);
            }
        }

        /// <summary>
        /// 创建Core项目
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="savePath"></param>
        /// <param name="modulePath"></param>
        public static void CreateBaseController(string templatePath, string savePath, ModuleModel moduleModel)
        {
            string guid = Guid.NewGuid().ToString();
            string text = FileHelper.FileRead(templatePath);
            var result = Engine.Razor.RunCompile(templateSource: text, name: guid, model: moduleModel);

            string path = savePath + $@"\{moduleModel.ModulePath}\{moduleModel.Name}.Web";
            string namePath = path + @$"\BaseController.cs";
            if (!FileHelper.IsExistFile(namePath))
            {
                if (!FileHelper.IsExistDirectory(path))
                {
                    FileHelper.CreateDirectory(path);
                }
                FileHelper.CreateFile(namePath, result);
            }
        }



        // <summary>
        /// 添加到目录
        /// </summary>
        /// <param name="CoreSlnPath"></param>
        /// <param name="GetSlnPath"></param>
        /// <param name="NameSpace"></param>
        /// <param name="typeEnum"></param>
        public static void AddSln(string templatePath, string savePath, ModuleModel moduleModel)
        {
            string guid = Guid.NewGuid().ToString();
            string text = File.ReadAllText(templatePath);
            var result = Engine.Razor.RunCompile(templateSource: text, name: guid, model: moduleModel);

            var slns = Directory.GetFiles(Directory.GetParent(savePath).FullName).Where(it => it.Contains(".sln"));
            if (slns.Any())
            {
                var sln = slns.First();

                FileStream fs = new FileStream(sln, FileMode.Append);
                var sw = new StreamWriter(fs);
                sw.WriteLine(result);
                sw.Close();
                fs.Close();
            }

        }

    }
}
