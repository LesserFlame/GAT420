using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class AutonomousAgent : Agent
{
    public Perception flockPerception;
    public ObstaclePerception obstaclePerception;
    public AutonomousAgentData data;
    public GameObject eye;

    public float wanderAngle { get; set; } = 0;
    void Update()
    {
        var gameObjects = perception.GetGameObjects();
        if(data.showSeekFleeLines)
        {
            foreach (var gameObject in gameObjects)
            {
                Debug.DrawLine(transform.position, gameObject.transform.position);
            }
        }
        
        if (gameObjects.Length > 0) 
        {
            movement.ApplyForce(Steering.Seek(this, gameObjects[0]) * data.seekWeight);
            movement.ApplyForce(Steering.Flee(this, gameObjects[0]) * data.fleeWeight);
        }

        gameObjects = flockPerception.GetGameObjects();
        if (data.showFlockLines)
        {
            foreach (var gameObject in gameObjects)
            {
                Debug.DrawLine(transform.position, gameObject.transform.position, Color.cyan);
            }
        }

        //flocking
        if (gameObjects.Length > 0) 
        {
            movement.ApplyForce(Steering.Cohesion(this, gameObjects) * data.cohesionWeight);
            movement.ApplyForce(Steering.Separation(this, gameObjects, data.separationRadius) * data.separationWeight);
            movement.ApplyForce(Steering.Alignment(this, gameObjects) * data.alignmentWeight);
        }

        //obstacle
        if (obstaclePerception.IsObstacleInFront())
        {
            Vector3 direction = obstaclePerception.GetOpenDirection();
            movement.ApplyForce(Steering.CalculateSteering(this, direction) * data.obstacleWeight);
        }

        //wander
        if (movement.acceleration.sqrMagnitude <= movement.maxForce * 0.1f)
        {
			movement.ApplyForce(Steering.Wander(this));
        }
		Vector3 position = transform.position;
		position = Utilities.Wrap(position, new Vector3(-20, -20, -20), new Vector3(20, 20, 20));
		position.y = 0;
		transform.position = position;

		///////////////////
		//eyeball
		gameObjects = flockPerception.GetGameObjects();
		//Debug.DrawRay(eye.transform.position, eye.transform.forward * 5, Color.blue);
		//var closest = Utilities.FindClosestObject(transform.position, gameObjects);

        Vector3 desiredLook;
        if (gameObjects.Length != 0) {
            desiredLook = gameObjects[0].transform.position - eye.transform.position; 
            //Debug.Log(closest.gameObject.name);
            //Debug.DrawLine(eye.transform.position, desiredLook);
        }
        else { desiredLook = transform.forward; }

        Quaternion rotation = Quaternion.LookRotation(desiredLook);
        eye.transform.rotation = Quaternion.Slerp(eye.transform.rotation, rotation, Time.deltaTime * 2); ;
        //Debug.DrawRay(eye.transform.position, eye.transform.forward * 4, Color.red);
		//eye.transform.rotation = Vector3.Slerp(eye.transform.forward, desiredLook, Time.deltaTime * 2);
	}
}