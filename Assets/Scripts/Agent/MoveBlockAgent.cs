using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class MoveBlockAgent : Agent
{
    [SerializeField] GameGrid grid;
    [SerializeField] ShapeStorage shapeStorage;

    void Start()
    {
        RequestDecision(); // yêu cầu AI hành động ngay khi start
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("🎯 AI nhận hành động");

        if (grid.isGameOver)
        {
            AddReward(-100f);
            return;
        }

        Shape shape = shapeStorage.shapeList[0];
        shapeStorage.SetCurrentSelectedShape(shape);

        var combos = grid.GetAllSquaresCombinations(shape.currentShapeData.rows, shape.currentShapeData.columns);

        foreach (var combo in combos)
        {
            List<int> filled = new List<int>();
            int index = 0;

            for (int r = 0; r < shape.currentShapeData.rows; r++)
            {
                for (int c = 0; c < shape.currentShapeData.columns; c++)
                {
                    if (shape.currentShapeData.board[r].column[c])
                        filled.Add(combo[index]);
                    index++;
                }
            }

            bool canPlace = true;
            foreach (int i in filled)
            {
                if (grid.GetGridSquareAtIndex(i).SquareOccupied)
                {
                    canPlace = false;
                    break;
                }
            }

            if (canPlace)
            {
                foreach (int i in filled)
                {
                    grid.GetGridSquareAtIndex(i).Selected = true;
                }

                GameEvents.CheckIfShapeCanBePlaced();
                AddReward(10f);
                Debug.Log("✅ AI đã đặt khối");
                return;
            }
        }

        AddReward(-1f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // test đơn giản: luôn chọn shape đầu
    }
}
