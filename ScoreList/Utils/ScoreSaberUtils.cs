using System.Collections;
using System.Collections.Generic;
using IPA.Loader;
using SiraUtil.Tools;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.Networking;

namespace ScoreList.Utils
{
    public static class ScoreSaberUtils
    {
        public class ScoreSaberImageUtil
        {
            public List<ScoreSaberImages> image;
        }

        public class ScoreSaberImages
        {
            public Sprite sprite;
            public string downloadURL;
        }
    }
}
