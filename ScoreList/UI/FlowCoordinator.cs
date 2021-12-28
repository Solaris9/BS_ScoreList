using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using ScoreList.Scores;
using System;
using System.Collections.Generic;
using SiraUtil.Logging;
using Zenject;

namespace ScoreList.UI {
    internal class ScoreListCoordinator : FlowCoordinator, IInitializable  {
        public static ScoreListCoordinator Instance;

        [Inject] private readonly SiraLog _siraLog; 
        [Inject] private readonly ScoreManager _scoreManager;

        [Inject] private ScoreViewController _scoreView;
        [Inject] private DetailViewController _detailView;
        [Inject] private FilterViewController _filterView;
        [Inject] private ConfigViewController _configView;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            try
            {
                if (!firstActivation) return;

                _scoreView.DidSelectSong += HandleDidSelectSong;
                Instance = this;

                SetTitle("ScoreList");
                showBackButton = true;

                ProvideInitialViewControllers(_scoreView, _filterView, _detailView, _configView);
            } catch (Exception ex) {
                _siraLog.Error(ex);
            }
        }

        private async void HandleDidSelectSong(int scoreId)
        {
            var score = await _scoreManager.GetScore(scoreId);
            await _detailView.Load(score);
        }

        public void ShowFilteredScores(List<BaseFilter> filters) => 
            _scoreView.FilterScores(filters);
        
        // flow coordinator stuff

        protected override void BackButtonWasPressed(ViewController _) => 
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);

        public void SetupUI()
        {
            var parentFlow = BeatSaberUI.MainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            BeatSaberUI.PresentFlowCoordinator(parentFlow, this);
        }

        private static Action _show;

        public void Initialize()
        {
            if (_show == null)
                MenuButtons.instance.RegisterButton(
                    new MenuButton("ScoreList", "Sort and filter all your scores to view your best and worst!",
                    () => _show()));

            _show = SetupUI;
        }
    }
}
