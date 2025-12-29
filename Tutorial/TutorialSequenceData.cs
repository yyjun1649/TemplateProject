using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial/Tutorial Sequence")]
public class TutorialSequenceData : ScriptableObject
{
    [Header("시퀀스 정보")]
    public string sequenceName;
        
    [Header("스텝 목록")]
    public List<TutorialStepData> steps = new List<TutorialStepData>();
}