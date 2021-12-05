using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using IPA.Utilities;
using ScoreList.Scores;
using System;
using Zenject;

namespace ScoreList.UI {
    class ScoreListCoordinator : FlowCoordinator {
        public static ScoreListCoordinator Instance;

        private FlowCoordinator parentFlowCoordinator;
        private ScoreViewController scoreView;
        private DetailViewController detailView;
        private FilterViewController filterView;

        public void ShowFilteredScores(SearchQuery query) => scoreView.FilterScores(query);

        public void Awake()
        {
            if (scoreView == null)
            {
                scoreView = BeatSaberUI.CreateViewController<ScoreViewController>();
                detailView = BeatSaberUI.CreateViewController<DetailViewController>();
                filterView = BeatSaberUI.CreateViewController<FilterViewController>();

                scoreView.didSelectSong += HandleDidSelectSong;
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            try {
                if (firstActivation) {
                    Instance = this;

                    SetTitle("ScoreList");
                    showBackButton = true;

                    ProvideInitialViewControllers(scoreView, filterView, detailView);
                }
            } catch (Exception ex) {
                Plugin.Log.Error(ex);
            }
        }

        internal async void HandleDidSelectSong(LeaderboardScore score)
        {
            await detailView.Load(score);
        }

        protected override void BackButtonWasPressed(ViewController topViewController) => 
            parentFlowCoordinator.DismissFlowCoordinator(this);
       
        public void SetParentFlowCoordinator(FlowCoordinator parent) => parentFlowCoordinator = parent;
    }

    class ScoreListUI : PersistentSingleton<ScoreListUI> {
        public MenuButton scoreListMenuButton;
        internal ScoreListCoordinator scoreListFlowCooridinator;

        internal void Setup() {
            scoreListMenuButton = new MenuButton("ScoreList", "Sort and filter all your scores to view your best and worst!", ScoreListMenuButtonPressed, true);
            MenuButtons.instance.RegisterButton(scoreListMenuButton);
        }

        private void ScoreListMenuButtonPressed() {
            if (scoreListFlowCooridinator == null)
                scoreListFlowCooridinator = BeatSaberUI.CreateFlowCoordinator<ScoreListCoordinator>();

            scoreListFlowCooridinator.SetParentFlowCoordinator(BeatSaberUI.MainFlowCoordinator);

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(scoreListFlowCooridinator, null, ViewController.AnimationDirection.Horizontal, true);
        }
    }
}
