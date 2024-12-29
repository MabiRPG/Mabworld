using Newtonsoft.Json;
using UnityEngine;
using System.Runtime.Serialization;
using System.Linq;

/// <summary>
///     Handles all training method processing.
/// </summary>
[JsonObject]
public class SkillTrainingMethod
{
    public TrainingMethodModel model;
    public IntManager count;

    public SkillTrainingMethod(int skillID, int methodID, string rank, string param1, string param2)
    {
        model = new TrainingMethodModel(GameManager.Instance.Database,
            skillID, methodID, rank, param1, param2);
        count = new IntManager();
    }

    public void Update<T>(T result, object caller, bool isSuccess)
    {
        if (count.Value >= model.countMax)
        {
            return;
        }

        int countAdd = CheckTraining(result as ResultHandler, caller, isSuccess);

        if (countAdd > 0)
        {
            count.Value += countAdd;
            Player.Instance.skillManager.Get(model.skillID).AddXP(model.xpGainEach * countAdd);
        }
    }

    public int CheckTraining<T>(T result, object caller, bool isSuccess) where T : ResultHandler
    {
        int countAdd = 0;

        switch (TrainingMethodTypeModel.FindByID(model.trainingMethodID))
        {
            case "Success":
                if (result.GetType() != typeof(ResultGatherController))
                {
                    break;
                }

                if (isSuccess)
                {
                    countAdd += 1;
                }

                break;
            case "Fail":
                if (result.GetType() != typeof(ResultGatherController))
                {
                    break;
                }

                if (!isSuccess)
                {
                    countAdd += 1;
                }

                break;
            case "Gather":
                {
                    if (result.GetType() != typeof(ResultGatherController))
                    {
                        break;
                    }

                    ResultGatherController gatherResult = result as ResultGatherController;

                    if (isSuccess && gatherResult.resourceID == int.Parse(model.param2))
                    {
                        countAdd += 1;
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

                    if (isSuccess
                        && gatherResult.resourceID == int.Parse(model.param2)
                        && resource.resource.Value == 0)
                    {
                        countAdd += 1;
                    }

                    break;
                }
            case "Craft":
                {
                    if (result.GetType() != typeof(ResultCraftController))
                    {
                        break;
                    }

                    ResultCraftController craftResult = result as ResultCraftController;

                    if (isSuccess
                        && craftResult.products.Any(v => v.itemID == int.Parse(model.param1)))
                    {
                        countAdd += craftResult.quantity;
                    }

                    break;
                }
            default:
                break;
        }

        return countAdd;
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