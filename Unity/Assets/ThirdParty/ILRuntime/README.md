### 1.ILRuntime注意事项 
使用ILRuntime需注意，ILRuntime自动生成BindingCode时，请务必将Unity、热更工程Hotfix加上“ILRuntime”宏，
然后在Unity菜单栏点击Tools/ILRuntime/Generate CLR Binding Code by Analysis,等待一会时间，
将会自动把热更中引用的类自动生成BindingCode放在ThirdParty/ILRuntime/Generated文件下，导出ios工程时，
请先将hotfix工程的属性改成Release模式，并勾选优化代码选项（不能勾选usafe代码，ILRuntime热更不支持）
并设置Strip Engine Code的勾选为空（不勾选），测试代码


	    private int curSecond = 0;

	    private async void SecondTimer()
	    {
	        TimerComponent timer = Game.Scene.GetComponent<TimerComponent>();
	        while (true)
	        {
	            await timer.WaitAsync(1000);
	            Debug.Log($"Current Second--{this.curSecond}");
	            loginBtn.GetComponentInChildren<Text>().text = $"timer--{this.curSecond}";
                curSecond++;
            }
	    }
		
在热更的UI逻辑代码Await里头调用SecondTimer函数，ios打包打印日志计时器正常输出