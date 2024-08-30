using Microsoft.WindowsAzure.ServiceRuntime;

namespace WebRole
{
    public class WebRole : RoleEntryPoint
    {
        private WebRoleCoreLauncher _webRoleCoreLauncher;

        public override bool OnStart()
        {
            _webRoleCoreLauncher = new WebRoleCoreLauncher();
            _webRoleCoreLauncher.Run();

            return base.OnStart();
        }

        public override void OnStop()
        {
            _webRoleCoreLauncher?.Dispose();
            base.OnStop();
        }
    }
}
