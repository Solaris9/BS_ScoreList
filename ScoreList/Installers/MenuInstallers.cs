using ScoreList.UI;
using SiraUtil;
using Zenject;

namespace ScoreList.Installers
{
    class MenuInstallers : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<DetailViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}