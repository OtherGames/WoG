using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleView : View
{
    public List<HingeJoint> actuators = new();
    public List<HingeJoint> rotary = new();

    public int velocityLimit = 500;
    public int rotateLimit = 1000;

    bool isSreering;
    public bool backRotate;

    private void Start()
    {
        GlobalEvents.onSteeringVehicle.AddListener(OnSteering);
        GlobalEvents.onStopSteeringVehicle.AddListener(Steering_Stoped);
    }

    private void Steering_Stoped()
    {
        isSreering = false;
    }

    private void OnSteering(VehicleView view)
    {
        if (view == this)
        {
            isSreering = true;
        }
    }

    private void Update()
    {
        if (!isSreering)
            return;

        //backRotate = false;

        if (Input.GetKey(KeyCode.W))
        {
            Gas(1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Gas(-1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Rotate(-1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A))
        {
            foreach (var joint in rotary)
            {
                var motor = joint.motor;

                motor.targetVelocity = 0;
                motor.force = 0;
                
                joint.motor = motor;
            }
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
        {
            foreach (var joint in actuators)
            {
                var motor = joint.motor;

                motor.targetVelocity = 0;
                motor.force = 0;

                joint.motor = motor;
            }
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            backRotate = true;
        }

        if (backRotate)
        {
            foreach (var joint in rotary)
            {
                var body = actuators.Find(a => a.connectedBody == joint.GetComponent<Rigidbody>());
                var motor = joint.motor;

                if (joint.angle > 3)
                {
                    motor.targetVelocity -= 10;
                    motor.force += 5;
                }
                else if(joint.angle < -3)
                {
                    motor.targetVelocity += 10;
                    motor.force += 5;
                }
                else
                {
                    motor.targetVelocity = 0;
                    motor.force = 0;

                }

                motor.targetVelocity = Mathf.Clamp(motor.targetVelocity, -rotateLimit, rotateLimit);
                motor.force = Mathf.Clamp(motor.force, 0, rotateLimit);

                joint.motor = motor;
            }
        }
    }

    

    private void Rotate(int dir)
    {
        backRotate = false;

        foreach (var joint in rotary)
        {
            var motor = joint.motor;
            float velocity = 0;
            if (!Mathf.Approximately(joint.axis.x, 0))
            {
                velocity = 1 * joint.axis.x * dir;
            }
            if (!Mathf.Approximately(joint.axis.y, 0))
            {
                velocity = 1 * joint.axis.y * dir;
            }
            if (!Mathf.Approximately(joint.axis.z, 0))
            {
                velocity = 1 * joint.axis.z * dir;
            }
            motor.targetVelocity += velocity;
            motor.force += 1;

            motor.targetVelocity = Mathf.Clamp(motor.targetVelocity, -rotateLimit, rotateLimit);
            motor.force = Mathf.Clamp(motor.force, 0, rotateLimit);

            joint.motor = motor;
        }
    }

    private void Gas(int dir)
    {
        //backRotate = true;
        foreach (var joint in actuators)
        {
            var motor = joint.motor;
            float velocity = 0;
            if (!Mathf.Approximately(joint.axis.x, 0))
            {
                velocity = 1 * joint.axis.x * dir;
            }
            if (!Mathf.Approximately(joint.axis.y, 0))
            {
                velocity = 1 * joint.axis.y * dir;
            }
            if (!Mathf.Approximately(joint.axis.z, 0))
            {
                velocity = 1 * joint.axis.z * dir;
            }
            motor.targetVelocity += velocity;
            motor.force += 1;

            motor.targetVelocity = Mathf.Clamp(motor.targetVelocity, -velocityLimit, velocityLimit);
            motor.force = Mathf.Clamp(motor.force, 0, velocityLimit);

            joint.motor = motor;
        }
    }
}
