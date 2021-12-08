using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using ScoreList.Scores;
using System;

namespace ScoreList.UI {
    class ScoreListCoordinator : FlowCoordinator {
        public static ScoreListCoordinator Instance;

        private FlowCoordinator _parentFlowCoordinator;
        private ScoreViewController _scoreView;
        private DetailViewController _detailView;
        private FilterViewController _filterView;
        private ConfigViewController _configView;

        public void ShowFilteredScores(SearchQuery query) => _scoreView.FilterScores(query);

        public void Awake()
        {
            if (_scoreView == null)
            {
                _scoreView = BeatSaberUI.CreateViewController<ScoreViewController>();
                _detailView = BeatSaberUI.CreateViewController<DetailViewController>();
                _filterView = BeatSaberUI.CreateViewController<FilterViewController>();
                _configView = BeatSaberUI.CreateViewController<ConfigViewController>();

                _scoreView.didSelectSong += HandleDidSelectSong;
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            try {
                if (firstActivation) {
                    Instance = this;

                    SetTitle("ScoreList");
                    showBackButton = true;

                    ProvideInitialViewControllers(
                        _scoreView,
                        _filterView,
                        _detailView,
                        _configView
                    );
                }
            } catch (Exception ex) {
                Plugin.Log.Error(ex);
            }
        }

        private async void HandleDidSelectSong(LeaderboardScore score)
        {
            await _detailView.Load(score);
        }

        protected override void BackButtonWasPressed(ViewController _) => 
            _parentFlowCoordinator.DismissFlowCoordinator(this);
       
        public void SetParentFlowCoordinator(FlowCoordinator parent) => _parentFlowCoordinator = parent;
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
