using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticle;
        public Axel axel;
    }

    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    private CarLights carLights;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;

        carLights = GetComponent<CarLights>();
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        WheelEffects();
    }

    void LateUpdate()
    {
        Move();
        Steer();
        Brake();
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    void GetInputs()
    {
        if(control == ControlMode.Keyboard)
        {
            // Get vertical input (W/Up = 1, S/Down = -1)
            float vertical = 0;
            if (Keyboard.current.wKey.isPressed) vertical += 1;
            if (Keyboard.current.sKey.isPressed) vertical -= 1;
            moveInput = vertical;

            // Get horizontal input (D/Right = 1, A/Left = -1)
            float horizontal = 0;
            if (Keyboard.current.dKey.isPressed) horizontal += 1;
            if (Keyboard.current.aKey.isPressed) horizontal -= 1;
            steerInput = horizontal;
        }
    }

    void Move()
    {
        foreach(var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach(var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if (Keyboard.current.spaceKey.isPressed || moveInput == 0)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
            }

            if (carLights != null)
            {
                carLights.isBackLightOn = true;
                carLights.OperateBackLights();
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }

            if (carLights != null)
            {
                carLights.isBackLightOn = false;
                carLights.OperateBackLights();
            }
        }
    }

    void AnimateWheels()
    {
        foreach(var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
            //var dirtParticleMainSettings = wheel.smokeParticle.main;

            if (wheel.wheelEffectObj != null && wheel.wheelCollider.isGrounded == true)
            {
                var trailRenderer = wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>();
                if (Keyboard.current.spaceKey.isPressed && wheel.axel == Axel.Rear && carRb.linearVelocity.magnitude >= 10.0f)
                {
                    if (trailRenderer != null)
                        trailRenderer.emitting = true;
                    if (wheel.smokeParticle != null)
                        wheel.smokeParticle.Emit(1);
                }
                else
                {
                    if (trailRenderer != null)
                        trailRenderer.emitting = false;
                }
            }
        }
    }}