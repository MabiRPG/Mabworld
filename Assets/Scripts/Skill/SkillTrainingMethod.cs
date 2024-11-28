using System;


[Serializable]
/// <summary>
///     Handles all training method processing.
/// </summary>
public class SkillTrainingMethod : TrainingMethodModel
{
    public IntManager count = new IntManager();

    // Skill instance
    private Skill skill;

    public SkillTrainingMethod(Skill skill, int methodID, string rank)
        : base(GameManager.Instance.Database, skill.ID, methodID, rank)
    {
        this.skill = skill;
        count.Value = 0;
    }

    public void Update<T>(T result, object caller, bool isSuccess)
    {
        if (count.Value >= countMax)
        {
            return;
        }

        if (CheckTraining(result as ResultHandler, caller, isSuccess))
        {
            count.Value += 1;
            skill.AddXP(xpGainEach);
        }
    }

    public bool CheckTraining<T>(T result, object caller, bool isSuccess) where T : ResultHandler
    {
        switch (TrainingMethodTypeModel.FindByID(trainingMethodID))
        {
            case "Success":
                if (result.GetType() != typeof(ResultGatherController))
                {
                    break;
                }
                return isSuccess;
            case "Fail":
                if (result.GetType() != typeof(ResultGatherController))
                {
                    break;
                }

                return !isSuccess;
            case "Gather":
                {
                    if (result.GetType() != typeof(ResultGatherController))
                    {
                        break;
                    }

                    ResultGatherController gatherResult = result as ResultGatherController;

                    if (isSuccess && gatherResult.resourceID == int.Parse(param2))
                    {
                        return true;
                    }

                    break;
                }
            case "Fully gather":
                {
                    if (result.GetType() != typeof(ResultGatherController))
                    {
                        break;
                    }

                    MapResource resource = (MapResource)caller;
                    ResultGatherController gatherResult = result as ResultGatherController;

                    if (gatherResult.resourceID == int.Parse(param2)
                        && resource.resource.Value == 0)
                    {
                        return true;
                    }

                    break;
                }
            default:
                break;
        }

        return false;
    }

    /// <summary>
    ///     Checks if the training method has been complete
    /// </summary>
    /// <returns>True if complete, False otherwise</returns>
    public bool IsComplete()
    {
        return count.Value == countMax;
    }

    public void Clear()
    {
        // Player.Instance.trainingEvent -= Update;
        count.Clear();
    }
}