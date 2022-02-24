using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Data.Abstractions.Adapter;
using Data.Abstractions.Entities;
using Data.Abstractions.Helper;
using Data.Abstractions.Judge;
using Data.Adapters.Mysql;
using Data.Adapters.Oracle;
using Data.Adapters.SqlServer;
using HandyControl.Controls;

using HandyControl.Tools.Extension;
using SL.Core.Tool.Data;
using SL.Core.Tool.UserControl;

namespace SL.Core.Tool.ViewModel
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        public MainWindowViewModel()
        {
            //选择路径
            this.SelectCodePath = new SimpleCommand(o => true, x => this.SelectCodePathEvent());
            //加载表
            this.LoadTable = new SimpleCommand(o => true, x => this.LoadTableEvent());
            //保存配置
            this.SaveConfig = new SimpleCommand(o => true, x => this.SaveConfigEvent());
            //保存配置
            this.LoadConfig = new SimpleCommand(o => true, x => this.LoadConfigEvent());
            //生成代码
            this.CreateCode = new SimpleCommand(o => true, x => this.CreateCodeEvent());
        }

        private int _dbType = GlobalData.Config.DbType;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public int DbType
        {
            get { return _dbType; }
            set { _dbType = value; NotifyPropertyChanged(); }
        }


        /// <summary>
        /// 数据库类型
        /// </summary>
        private DbProvider _dbProvider = (DbProvider)GlobalData.Config.DbType;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbProvider DbProvider
        {
            get { return _dbProvider; }
            set { _dbProvider = value; NotifyPropertyChanged(); }
        }

        private List<OptionModel> _dbProviderEnum = DbProvider.SqlServer.ToResult();

        public List<OptionModel> DbProviderEnum
        {
            get { return _dbProviderEnum; }
            set { _dbProviderEnum = value; NotifyPropertyChanged(); }
        }


        private string _dataConnection = GlobalData.Config.DataConnection;

        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        public string DataConnection
        {
            get { return _dataConnection; }
            set { _dataConnection = value; NotifyPropertyChanged(); }
        }


        private List<DbTable> _dbTables = null;
        /// <summary>
        /// 表集合
        /// </summary>
        public List<DbTable> DbTables
        {
            get { return _dbTables; }
            set { _dbTables = value; NotifyPropertyChanged(); }
        }

        private string _tableStr;
        /// <summary>
        /// 表集合
        /// </summary>
        public string TableStr
        {
            get { return _tableStr; }
            set { _tableStr = value; NotifyPropertyChanged(); }
        }



        private string _module = GlobalData.Config.Module;

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Module
        {
            get { return _module; }
            set { _module = value; NotifyPropertyChanged(); }
        }

        //private string _cSavePath = GlobalData.Config.CSavePath;

        ///// <summary>
        ///// 控制器生成地址
        ///// </summary>
        //public string CSavePath
        //{
        //    get { return _cSavePath; }
        //    set { _cSavePath = value; NotifyPropertyChanged(); }
        //}

        //private string _dSavePath = GlobalData.Config.DSavePath;

        ///// <summary>
        ///// 仓储层代码地址
        ///// </summary>
        //public string DSavePath
        //{
        //    get { return _dSavePath; }
        //    set { _dSavePath = value; NotifyPropertyChanged(); }
        //}

        private string _saveCodePath = GlobalData.Config.SaveCodePath;

        /// <summary>
        /// 仓储层代码地址
        /// </summary>
        public string SaveCodePath
        {
            get { return _saveCodePath; }
            set { _saveCodePath = value; NotifyPropertyChanged(); }
        }

        bool _isEnum = false;
        /// <summary>
        /// 是否枚举
        /// </summary>
        public bool IsEnum
        {
            get { return _isEnum; }
            set
            {
                _isEnum = value;
                NotifyPropertyChanged();
            }
        }

        bool _isController = false;
        /// <summary>
        /// 是否控制器
        /// </summary>
        public bool IsController
        {
            get { return _isController; }
            set
            {
                _isController = value;
                NotifyPropertyChanged();
            }
        }

        bool _isDDD = false;
        /// <summary>
        /// 是否控制器
        /// </summary>
        public bool IsDDD
        {
            get { return _isDDD; }
            set
            {
                _isDDD = value;
                NotifyPropertyChanged();
            }
        }


        bool _isNewProject = false;
        /// <summary>
        /// 是否新项目
        /// </summary>
        public bool IsNewProject
        {
            get { return _isNewProject; }
            set
            {
                _isNewProject = value;
                NotifyPropertyChanged();
            }
        }




        #region Event
      

        /// <summary>
        /// 选择控制器路径
        /// </summary>
        public ICommand SelectCodePath { get; }

        /// <summary>
        /// 选择控制器路径-事件
        /// </summary>
        private void SelectCodePathEvent()
        {
            System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)//注意，此处一定要手动引入System.Window.Forms空间，否则你如果使用默认的DialogResult会发现没有OK属性
            {
                SaveCodePath = openFileDialog.SelectedPath;
            }
        }




        /// <summary>
        /// 加载表
        /// </summary>
        public ICommand LoadTable { get; }
        /// <summary>
        /// 加载表-事件
        /// </summary>
        private async void LoadTableEvent()
        {
            var dialog = Dialog.Show(new ProLoadIngDialog("数据加载中"));
            try
            {
                await Task.Run(async () =>
                {
                    switch (DbProvider)
                    {
                        case DbProvider.SqlServer:
                            DbTables = await new SqlServerDbAdapter().GetDbTables(this.DataConnection);

                            break;
                        case DbProvider.MySql:
                            DbTables = await new MysqlDbAdapter().GetDbTables(this.DataConnection);

                            break;
                        case DbProvider.Oracle:
                            DbTables = await new OracleDbAdapter().GetDbTables(this.DataConnection);

                            break;
                    }
                });
                Growl.Success("数据加载成功");
            }
            catch (Exception ex)
            {
                Growl.Error($"数据加载失败-{ex.Message}");
            }
            finally
            {
                dialog.Close();
            }

        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public ICommand SaveConfig { get; }
        /// <summary>
        /// 保存配置-事件
        /// </summary>
        private async void SaveConfigEvent()
        {
            try
            {
                await Task.Run(() =>
                {
                    AppConfig appConfig = new AppConfig();
                    appConfig.DbType = DbType;
                    appConfig.DataConnection = DataConnection;
                    appConfig.SaveCodePath = SaveCodePath;
                    appConfig.Module = Module;
                    GlobalData.Config = appConfig;
                    GlobalData.Save();
                });
                Growl.Success("配置保存成功");
            }
            catch (Exception ex)
            {
                Growl.Error($"配置保存失败-{ex.Message}");
            }
            finally
            {

            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public ICommand LoadConfig { get; }

        /// <summary>
        /// 保存配置-事件
        /// </summary>
        private async void LoadConfigEvent()
        {
            try
            {
                await Task.Run(() =>
                {
                    GlobalData.Init();
                    DbType = GlobalData.Config.DbType;
                    DataConnection = GlobalData.Config.DataConnection;
                    SaveCodePath = GlobalData.Config.SaveCodePath;
                    Module = GlobalData.Config.Module;
                });
                Growl.Success("配置加载成功");
            }
            catch (Exception ex)
            {
                Growl.Error($"配置加载失败-{ex.Message}");
            }
            finally
            {

            }
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        public ICommand CreateCode { get; }
        /// <summary>
        /// 生成代码-事件
        /// </summary>
        private async void CreateCodeEvent()
        {
            var dialog = Dialog.Show(new ProLoadIngDialog("代码生成中"));
            try
            {
                
                  await Task.Run(() =>
                {
                    if (!IsDDD && !IsEnum && !IsController && !IsNewProject)
                    {
                        Growl.Warning($"请先勾选需要生成的操作");
                        return;
                    }
                    if (Module.IsNull())
                    {
                        Growl.Warning($"模块名称不能为空");
                        return;
                    }
                    if (SaveCodePath.IsNull())
                    {
                        Growl.Warning($"保存路径不能为空");
                        return;
                    }

                    var path = AppContext.BaseDirectory;
                    string id = Module.Split("_")[0];
                    string name = Module.Split("_")[1];
                    ModuleModel moduleModel = new ModuleModel
                    {
                        Id = id,
                        Name = name,
                        PrefixSpace = GlobalData.Config.PrefixSpace
                    };
                    #region 01-生成项目
                    if (IsNewProject)
                    {
                        #region 01.新项目生成
                        //00.创建Directory.Build
                        string directoryBuildTemplatePath = path + @"\Template\Core\NewProject\Directory.Build.props.txt";
                        ModuleHelper.CreateDirectoryBuild(directoryBuildTemplatePath, SaveCodePath, moduleModel);

                        //01.创建Core
                        string coreTemplatePath = path + @"\Template\Core\NewProject\Core.csproj.txt";
                        ModuleHelper.CreateCodeModule(coreTemplatePath, SaveCodePath, moduleModel);

                        //02.创建Web
                        //02.00.项目文件
                        string webTemplatePath = path + @"\Template\Core\NewProject\Web.csproj.txt";
                        ModuleHelper.CreateWebModule(webTemplatePath, SaveCodePath, moduleModel);
                        //02.01.控制器
                        string controllerTemplatePath = path + @"\Template\Core\NewProject\Web.BaseController.txt";
                        ModuleHelper.CreateBaseController(controllerTemplatePath, SaveCodePath, moduleModel);
                        #endregion

                        #region 02.更新依赖
                        string slnPath = path + @"\Template\Core\NewProject\Sln.txt";
                        ModuleHelper.AddSln(slnPath, SaveCodePath, moduleModel);
                        #endregion
                        Growl.Success("01.项目生成成功");
                    }

                    #endregion

                    #region 主代码生成
                    if (IsController || IsDDD)
                    {
                        if (DbTables == null)
                        {
                            Growl.Warning($"请先选择表数据");
                            return;
                        }
                        var createTables = DbTables.Where(a => a.Selected == true).ToList();

                        if (IsDDD)
                        {
                            string domainBasePath = SaveCodePath + @$"\{Module}\{moduleModel.Name}.Core\Domain\";

                            string infrastructureBasePath = SaveCodePath + @$"\{Module}\{moduleModel.Name}.Core\Infrastructure\";

                            string applicationBasePath = SaveCodePath + @$"\{Module}\{moduleModel.Name}.Core\Application\";

                            foreach (var item in createTables)
                            {
                            
                                item.ModuleModel = moduleModel;
                                #region 01.Domain
                                //Entity                           
                                string entityTemplatePath = path + @"\Template\Core\DDD\Domain\Entity.txt";
                                ModuleHelper.CreateFileCommon(entityTemplatePath, $"{domainBasePath}{item.EntityName}",
                                    $"{item.EntityName}Entity.cs", item);

                                //Entity.Extend
                                string entityExtendTemplatePath = path + @"\Template\Core\DDD\Domain\Entity.Extend.txt";
                                ModuleHelper.CreateFileCommon(entityExtendTemplatePath, $"{domainBasePath}{item.EntityName}",
                                  $"{item.EntityName}Entity.Extend.cs", item);

                                //IRepository
                                string irepositoryTemplatePath = path + @"\Template\Core\DDD\Domain\IRepository.txt";
                                ModuleHelper.CreateFileCommon(irepositoryTemplatePath, $"{domainBasePath}{item.EntityName}",
                                  $"I{item.EntityName}Repository.cs", item);
                                #endregion

                                #region 02.Infrastructure
                                //Repository
                                string repositoryTemplatePath = path + @"\Template\Core\DDD\Infrastructure\Repositories\Repository.txt";
                                ModuleHelper.CreateFileCommon(repositoryTemplatePath, $@"{infrastructureBasePath}Repositories\{item.EntityName}",
                               $"{item.EntityName}Repository.cs", item);

                                //ModuleServicesConfigurator
                                string moduleServicesConfiguratorTemplatePath = path + @"\Template\Core\DDD\Infrastructure\ModuleServicesConfigurator.txt";
                                ModuleHelper.CreateFileCommon(moduleServicesConfiguratorTemplatePath, $@"{infrastructureBasePath}",
                              $"ModuleServicesConfigurator.cs", item);

                                #endregion
                                #region 03.Application
                                //IService
                                string iserviceTemplatePath = path + @"\Template\Core\DDD\Application\IService.txt";
                                ModuleHelper.CreateFileCommon(iserviceTemplatePath, $@"{applicationBasePath}{item.EntityName}",
                                 $"I{item.EntityName}Service.cs", item);

                                //Service
                                string serviceTemplatePath = path + @"\Template\Core\DDD\Application\Service.txt";
                                ModuleHelper.CreateFileCommon(serviceTemplatePath, $@"{applicationBasePath}{item.EntityName}",
                                $"{item.EntityName}Service.cs", item);

                                //MapperConfig
                                string mapperConfigTemplatePath = path + @"\Template\Core\DDD\Application\MapperConfig.txt";
                                ModuleHelper.CreateFileCommon(mapperConfigTemplatePath, $@"{applicationBasePath}{item.EntityName}",
                               $"_MapperConfig.cs", item);

                                //Dto
                                //in
                                //In_QueryDto,In_AddDto,In_ImportDto
                                string inQueryDtoTemplatePath = path + @"\Template\Core\DDD\Application\Dto\In_QueryDto.txt";
                                ModuleHelper.CreateFileCommon(inQueryDtoTemplatePath, $@"{applicationBasePath}{item.EntityName}\Dto",
                             $"{item.EntityName}QueryDto.cs", item);

                                string inAddDtoTemplatePath = path + @"\Template\Core\DDD\Application\Dto\In_AddDto.txt";
                                ModuleHelper.CreateFileCommon(inAddDtoTemplatePath, $@"{applicationBasePath}{item.EntityName}\Dto",
                          $"{item.EntityName}AddDto.cs", item);

                                string inImportDtoTemplatePath = path + @"\Template\Core\DDD\Application\Dto\In_ImportDto.txt";
                                ModuleHelper.CreateFileCommon(inImportDtoTemplatePath, $@"{applicationBasePath}{item.EntityName}\Dto",
                         $"{item.EntityName}ImportDto.cs", item);

                                //Out_Dto,Out_ListDto,Out_ExportDto
                                string outDtoTemplatePath = path + @"\Template\Core\DDD\Application\Dto\Out_Dto.txt";

                                ModuleHelper.CreateFileCommon(outDtoTemplatePath, $@"{applicationBasePath}{item.EntityName}\Dto",
                       $"{item.EntityName}Dto.cs", item);

                                string outListDtoTemplatePath = path + @"\Template\Core\DDD\Application\Dto\Out_ListDto.txt";
                                ModuleHelper.CreateFileCommon(outListDtoTemplatePath, $@"{applicationBasePath}{item.EntityName}\Dto",
                     $"{item.EntityName}ListDto.cs", item);

                                string outExportDtoTemplatePath = path + @"\Template\Core\DDD\Application\Dto\Out_ExportDto.txt";
                                ModuleHelper.CreateFileCommon(outExportDtoTemplatePath, $@"{applicationBasePath}{item.EntityName}\Dto",
                    $"{item.EntityName}ExportDto.cs", item);
                                #endregion


                            }
                        }
                        if (IsController)
                        {
                            string controllerBasePath = SaveCodePath + @$"\{Module}\{moduleModel.Name}.Web\Controllers";

                            foreach (var item in createTables)
                            {
                                string controllerTemplatePath = path + @"\Template\Core\Controller\Controllers.txt";
                                ModuleHelper.CreateFileCommon(controllerTemplatePath, $@"{controllerBasePath}",
                                $"{item.EntityName}Controller.cs", item);
                            }
                           

                        }
                        Growl.Success("02.代码生成成功");
                    }

                    #endregion
                });

            }
            catch (Exception ex)
            {
                Growl.Error($"代码生成失败-{ex.Message}");
            }
            finally
            {
                dialog.Close();
                await Task.CompletedTask;
            }
        }

        #endregion

    }
}
