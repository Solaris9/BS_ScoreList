using ScoreList.UI;
using SiraUtil;
using Zenject;

namespace ScoreList.Installers
{
    public class MenuInstallers : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<FilterViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ScoreViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<DetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ConfigViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}