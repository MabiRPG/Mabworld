using Newtonsoft.Json;
using UnityEngine;
using System.Runtime.Serialization;

/// <summary>
///     Handles all training method processing.
/// </summary>
[JsonObject]
public class SkillTrainingMethod
{
    public TrainingMethodModel model;
    public IntManager count;

    public SkillTrainingMethod(int skillID, int methodID, string rank)
    {
        model = new TrainingMethodModel(GameManager.Instance.Database,
            skillID, methodID, rank);
        count = new IntManager();
    }

    public void Update<T>(T result, object caller, bool isSuccess)
    {
        if (count.Value >= model.countMax)
        {
            return;
        }

        if (CheckTraining(result as ResultHandler, caller, isSuccess))
        {
            count.Value += 1;
            Player.Instance.skillManager.Get(model.skillID).AddXP(model.xpGainEach);
        }
    }

    public bool CheckTraining<T>(T result, object caller, bool isSuccess) where T : ResultHandler
    {
        switch (TrainingMethodTypeModel.FindByID(model.trainingMethodID))
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

                    if (isSuccess && gatherResult.resourceID == int.Parse(model.param2))
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

                    if (gatherResult.resourceID == int.Parse(model.param2)
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
        return count.Value == model.countMax;
    }

    public void Clear()
    {
        count.Clear();
    }
}