using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Action;
using RAIN.Core;

[RAINAction]
public class IsPlayer : RAINAction
{
    public override void Start(RAIN.Core.AI ai)
    {
        base.Start(ai);
    }

    public override ActionResult Execute(RAIN.Core.AI ai)
    {
        var enemy = ai.WorkingMemory.GetItem<GameObject>("playerobj");
        if(enemy!=null)
        { 
            var enemyPos = enemy.transform.position;
            var distance = Vector3.Distance(ai.Body.transform.position, enemyPos);
            if (distance<=3)
            {
                ai.WorkingMemory.SetItem<bool>("attack",true);
            }
            else
            {
                ai.WorkingMemory.SetItem<bool>("attack", false);
            }
        }
        else
        {
            ai.WorkingMemory.SetItem<bool>("attack", false);
        }
        return ActionResult.SUCCESS;
    }

    public override void Stop(RAIN.Core.AI ai)
    {
        base.Stop(ai);
    }
}