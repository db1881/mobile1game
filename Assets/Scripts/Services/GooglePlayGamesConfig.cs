using UnityEngine;

namespace BalloonPop.Services
{
    [CreateAssetMenu(fileName = "GooglePlayGamesConfig", menuName = "Balloon Pop/Google Play Games Config")]
    public sealed class GooglePlayGamesConfig : ScriptableObject
    {
        [Tooltip("Google Play Console'da oluşturulan 'Total Stars' liderlik tablosunun kimliği.")]
        [SerializeField] private string totalStarsLeaderboardId = "";

        public string TotalStarsLeaderboardId => totalStarsLeaderboardId != null
            ? totalStarsLeaderboardId.Trim()
            : string.Empty;
    }
}
