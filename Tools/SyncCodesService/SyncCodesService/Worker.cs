namespace SyncCodesService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _workSpace;
        private FileSystemWatcher _watcher;
        private Dictionary<string, string> _codeNames;
        private List<string> _refreshed = new List<string>();
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            if (string.IsNullOrEmpty(Args.WorkPlace))
            {
                _workSpace = config["MyConfig:WorkSpace"];
            }
            else
            {
                _workSpace = Args.WorkPlace;
            }

            DirectoryInfo workDir = new DirectoryInfo(_workSpace);
            if (!workDir.Exists)
            {
                _logger.LogError($"目录{_workSpace}不存在,检查参数");
            }
            _workSpace = workDir.FullName.Replace("\\", "/");
            string codesRoot = Path.Combine(_workSpace, "Codes");
            _watcher = new FileSystemWatcher(codesRoot);
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _codeNames = new Dictionary<string, string>()
            {
                {"Model", _workSpace + "/Unity.Model.csproj"},
                {"ModelView", _workSpace + "/Unity.ModelView.csproj"},
                {"Hotfix", _workSpace + "/Unity.Hotfix.csproj"},
                {"HotfixView", _workSpace + "/Unity.HotfixView.csproj"},
            };

            //开始就刷一遍
            _logger.LogInformation($"已开启监听服务。");
            Refresh("", true);

            _logger.LogInformation($"正在监听目录{new DirectoryInfo(codesRoot).FullName}，按ESC可退出。");

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _refreshed.Clear();
                await Task.Delay(1000, stoppingToken);
            }
            _watcher.EnableRaisingEvents = false;
            Environment.Exit(0);
        }


        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Refresh(e.FullPath);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Refresh(e.FullPath);
        }

        private void Refresh(string path, bool force = false)
        {
            if (!force && Path.GetExtension(path).ToLower() != ".cs")
            {
                return;
            }
            path = path.Replace("\\", "/");
            foreach (var codeName in _codeNames)
            {
                string name = codeName.Key;
                string csproj = codeName.Value;
                string srcDir = $"{_workSpace}/Codes/{name}/";
                if (force || path.StartsWith(srcDir) && !_refreshed.Contains(name))
                {
                    AdjustTool.Adjust(_workSpace, csproj, srcDir);
                    _refreshed.Add(name);
                }
            }
        }
    }
}