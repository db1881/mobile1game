using UnityEngine;

namespace BalloonPop.Services
{
    [CreateAssetMenu(fileName = "GooglePlayGamesConfig", menuName = "Balloon Pop/Google Play Games Config")]
    public sealed class GooglePlayGamesConfig : ScriptableObject
    {
        [Tooltip("Google Play Console'da oluşturulan 'En Yüksek Skor' liderlik tablosunun kimliği.")]
        [SerializeField] private string highScoreLeaderboardId = "";

        public string HighScoreLeaderboardId => highScoreLeaderboardId != null
            ? highScoreLeaderboardId.Trim()
            : string.Empty;
    }
}
