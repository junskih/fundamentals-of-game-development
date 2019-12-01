// Save data class
[System.Serializable]
public class Save {

    public int level;
    public int totalScore;

    public Save(int levelInt, int totalScoreInt) {
        level = levelInt;
        totalScore = totalScoreInt;
    }
}
