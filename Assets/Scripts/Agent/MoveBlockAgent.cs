using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class NewMonoBehaviourScript : Agent
{
    [SerializeField] Grid grid;
    [SerializeField] ShapeStorage shapeStorage;
    public override void CollectObservations(VectorSensor sensor)
    {
        // float[81], 1 for active square, 0 for inactive
        sensor.AddObservation(grid.GetActiveGridSquares()); 

        // int*3 for each shape current data index
        foreach (Shape shape in shapeStorage.shapeList)
        {
            sensor.AddObservation(shape.shapeIndex); 
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }
}
