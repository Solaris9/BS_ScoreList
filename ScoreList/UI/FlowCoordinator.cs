using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using ScoreList.Scores;
using System;
using System.Collections.Generic;
using SiraUtil.Logging;
using Zenject;

#pragma warning disable CS0649

namespace ScoreList.UI
{
    internal class ScoreListCoordinator : FlowCoordinator, IInitializable
    {
        [Inject] readonly SiraLog _siraLog; 
        [Inject] readonly ScoreManager _scoreManager;

        [Inject] readonly ScoreViewController _scoreView;
        [Inject] readonly DetailViewController _detailView;
        [Inject] readonly FilterViewController _filterView;
        [Inject] readonly ConfigViewController _configView;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (!firstActivation) return;

                SetTitle("ScoreList");
                showBackButton = true;
                
                _scoreView.DidSelectSong += HandleDidSelectSong;

                ProvideInitialViewControllers(_scoreView, _filterView, _detailView, _configView);
            } catch (Exception ex)
            {
                _siraLog.Error(ex);
            }
        }

        protected override void BackButtonWasPressed(ViewController _) => 
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);

        public void SetupUI()
        {
            var parentFlow = BeatSaberUI.MainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            parentFlow.PresentFlowCoordinator(this);
        }

        static Action _show;

        public void Initialize()
        {
            if (_show == null)
            {
                var btn = new MenuButton(
                    "ScoreList",
                    "Sort and filter all your scores to view your best and worst!",
                    () => _show?.Invoke());
                MenuButtons.instance.RegisterButton(btn);
            }

            _show = SetupUI;
        }
        
        // other methods

        private async void HandleDidSelectSong(int scoreId)
        {
            var score = await _scoreManager.GetScore(scoreId);
            await _detailView.Load(score);
        }
    }
}
