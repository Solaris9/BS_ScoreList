using System.Threading.Tasks;

namespace ScoreList.Utils
{
    public class UserIDUtils
    {
        private IPlatformUserModel _userModel;
        
        public UserIDUtils(IPlatformUserModel userModel)
        {
            _userModel = userModel;
        }

        public int UserAsync()
        {
            var userId = _userModel.GetUserInfo().Id;
            return userId;
        }
    }
}