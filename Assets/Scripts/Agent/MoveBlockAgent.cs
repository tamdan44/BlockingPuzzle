using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class NewMonoBehaviourScript : Agent
{
    [SerializeField] Grid grid;
    [SerializeField] ShapeStorage shapeStorage;

    public override void OnEpisodeBegin()
    {
        grid.ClearGrid();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // float[81], 1 for unoccupied grid square, 0 for occupied
        sensor.AddObservation(grid.GetActiveGridSquares());

        // int*3 for each shape current data index
        foreach (Shape shape in shapeStorage.shapeList)
        {
            sensor.AddObservation(shape.shapeIndex); // TODO shape dissapeared
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // int chosenShape = actions.DiscreteActions[0];
        // TODO: choose 1 shape out of given shapes, do action PlaceShapeOnBoard

        if (grid.isGameOver)
        {
            AddReward(-100f);
        }
    }

    //Reward: -1 for put shape not work, +10 for each 10 scores, -100 for gameOver

}
