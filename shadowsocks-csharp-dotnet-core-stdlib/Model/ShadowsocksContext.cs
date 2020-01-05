using Shadowsocks.Std.Sys;
using Shadowsocks.Std.Util;
using Shadowsocks.Std.Util.Resource;

namespace Shadowsocks.Std.Model
{
    public class ShadowsocksContext
    {
        public string[] runArgs { get; private set; }

        public I18N i18N { get; private set; }

        public IApplication application { get; private set; }

        public IAutoStartup autoStartup { get; private set; }

        public IDelegatesInit delegatesInit { get; private set; }

        public ShadowsocksContext(string[] runArgs, IApplication application, I18N i18N, IAutoStartup autoStartup, IDelegatesInit delegatesInit)
        {
            this.runArgs = runArgs;
            this.application = application;
            this.i18N = i18N;
            this.autoStartup = autoStartup;
            this.delegatesInit = delegatesInit;

            this.delegatesInit.Init();
        }
    }
}