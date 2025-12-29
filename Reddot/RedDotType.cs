using Cysharp.Text;

namespace Library
{
    public static class RedDotPath
    {
        public const string Separator = "/";
        
        public const string MainTab = "MainTab";
        public const string ContentTab = "ContentTab";
        public const string ShopTab = "ShopTab";
        
        public const string StageReward = "MainTab/StageReward";
        
        
        /// <summary>
        /// 동적 경로 생성: 스테이지 보상 (특정 스테이지 전체)
        /// </summary>
        public static string GetStageRewardPath(int stageId)
        {
            return ZString.Format("{0}/{1}",StageReward,stageId);
        }

        /// <summary>
        /// 동적 경로 생성: 스테이지 섹션 보상 (특정 스테이지의 특정 섹션)
        /// </summary>
        public static string GetStageSectionRewardPath(int stageId, int sectionId)
        {
            return ZString.Format("{0}/{1}/{2}",StageReward,stageId,sectionId);
        }

        /// <summary>
        /// 경로에서 부모 경로 추출
        /// </summary>
        public static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            int lastSeparator = path.LastIndexOf(Separator);
            if (lastSeparator <= 0)
                return null;

            return path.Substring(0, lastSeparator);
        }

        /// <summary>
        /// 두 경로가 부모-자식 관계인지 확인
        /// </summary>
        public static bool IsChildOf(string childPath, string parentPath)
        {
            if (string.IsNullOrEmpty(childPath) || string.IsNullOrEmpty(parentPath))
                return false;

            return childPath.StartsWith(parentPath + Separator);
        }

        /// <summary>
        /// 경로의 깊이 (레벨) 반환
        /// </summary>
        public static int GetDepth(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;

            return path.Split(Separator[0]).Length;
        }
    }
}
