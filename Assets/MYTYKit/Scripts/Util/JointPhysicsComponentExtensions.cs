using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MYTYKit
{
    public class JointPhysicsSetting
    {
        public static readonly Dictionary<Type, Func<Object,Dictionary<Transform, int>, JObject>> SerializeActions = new ()
        {
            {typeof(HingeJoint2D), (joint2D, transformMap) 
                => (joint2D as HingeJoint2D).SerializeToJObject(transformMap) },
            {typeof(SpringJoint2D),  (joint2D, transformMap) 
                => (joint2D as SpringJoint2D).SerializeToJObject(transformMap) },
            {typeof(Rigidbody2D),  (rigidBody2D, _) 
                => (rigidBody2D as Rigidbody2D).SerializeToJObject() },
            {typeof(BoxCollider2D),  (boxCollider2D, _) 
                => (boxCollider2D as BoxCollider2D).SerializeToJObject() },

        };

        public static readonly Dictionary<string, Action<Object, JObject, Dictionary<int, Transform>>> DeserializeActions = new()
        {
            {nameof(HingeJoint2D), (joint2D, jObject, idTransformMap) 
                => (joint2D as HingeJoint2D).DeserializeFromJObject(jObject,idTransformMap) },
            {nameof(SpringJoint2D), (joint2D, jObject, idTransformMap) 
                => (joint2D as SpringJoint2D).DeserializeFromJObject(jObject,idTransformMap) },
            {nameof(Rigidbody2D), (rigidBody2D, jObject, _) 
                => (rigidBody2D as Rigidbody2D).DeserializeFromJObject(jObject) },
            {nameof(BoxCollider2D), (boxCollider2D, jObject, _) 
                => (boxCollider2D as BoxCollider2D).DeserializeFromJObject(jObject) },
        };
    }
    public static class JointPhysicsComponentExtensions
    {
        public static JObject SerializeToJObject(this HingeJoint2D hingeJoint2D, Dictionary<Transform, int> transformMap)
        {
            return JObject.FromObject(new//connected body should be implemented
            {
                hingeJoint2D.limits,
                hingeJoint2D.enableCollision,
                hingeJoint2D.autoConfigureConnectedAnchor,
                anchor = new {
                    hingeJoint2D.anchor.x,
                    hingeJoint2D.anchor.y
                },
                connectedAnchor = new
                {
                    hingeJoint2D.connectedAnchor.x,
                    hingeJoint2D.connectedAnchor.y
                },
                hingeJoint2D.useMotor,
                hingeJoint2D.motor,
                hingeJoint2D.useLimits,
                hingeJoint2D.breakForce,
                hingeJoint2D.breakTorque,
                connectedBody = hingeJoint2D.connectedBody==null ? -1 : transformMap[hingeJoint2D.connectedBody.transform]
            });
        }

        public static void DeserializeFromJObject(this HingeJoint2D hingeJoint2D,JObject jObject,
            Dictionary<int, Transform> idTransformMap)
        {
            hingeJoint2D.limits = jObject["limits"].ToObject<JointAngleLimits2D>();
            hingeJoint2D.enableCollision = (bool)jObject["enableCollision"];
            hingeJoint2D.autoConfigureConnectedAnchor = (bool)jObject["autoConfigureConnectedAnchor"];
            hingeJoint2D.anchor = jObject["anchor"].ToObject<Vector2>();
            hingeJoint2D.connectedAnchor = jObject["connectedAnchor"].ToObject<Vector2>();
            hingeJoint2D.useMotor = (bool)jObject["useMotor"];
            hingeJoint2D.motor = jObject["motor"].ToObject<JointMotor2D>();
            hingeJoint2D.useLimits = (bool)jObject["useLimits"];
            hingeJoint2D.breakForce = (float)jObject["breakForce"];
            hingeJoint2D.breakTorque = (float)jObject["breakTorque"];

            var id = (int)jObject["connectedBody"];
            if (id < 0) return;
            hingeJoint2D.connectedBody = idTransformMap[id].GetComponent<Rigidbody2D>();
        }
        public static JObject SerializeToJObject(this SpringJoint2D springJoint2D, Dictionary<Transform, int> transformMap)
        {
            return JObject.FromObject(new
            {
                springJoint2D.enableCollision,
                springJoint2D.autoConfigureConnectedAnchor,
                anchor = new
                {
                    springJoint2D.anchor.x,
                    springJoint2D.anchor.y
                },
                connectedAnchor = new
                {
                    springJoint2D.connectedAnchor.x,
                    springJoint2D.connectedAnchor.y
                },
                springJoint2D.distance,
                springJoint2D.dampingRatio,
                springJoint2D.frequency,
                springJoint2D.breakForce,
                connectedBody = springJoint2D.connectedBody==null ? -1 : transformMap[springJoint2D.connectedBody.transform]
            });
        }
        
        public static void DeserializeFromJObject(this SpringJoint2D springJoint2D,JObject jObject,
            Dictionary<int, Transform> idTransformMap)
        {
            
            springJoint2D.enableCollision = (bool)jObject["enableCollision"];
            springJoint2D.autoConfigureConnectedAnchor = (bool)jObject["autoConfigureConnectedAnchor"];
            springJoint2D.anchor = jObject["anchor"].ToObject<Vector2>();
            springJoint2D.connectedAnchor = jObject["connectedAnchor"].ToObject<Vector2>();
            springJoint2D.distance = (float)jObject["distance"];
            springJoint2D.dampingRatio = (float)jObject["dampingRatio"];
            springJoint2D.frequency = (float)jObject["frequency"];
            springJoint2D.breakForce = (float)jObject["breakForce"];
            var id = (int)jObject["connectedBody"];
            if (id < 0) return; 
            springJoint2D.connectedBody = idTransformMap[id].GetComponent<Rigidbody2D>();
        }

        public static JObject SerializeToJObject(this Rigidbody2D rigidBody2D)
        {
            return JObject.FromObject(new
            {
                rigidBody2D.bodyType,
                rigidBody2D.simulated,
                rigidBody2D.useAutoMass,
                rigidBody2D.mass,
                rigidBody2D.drag,
                rigidBody2D.angularDrag,
                rigidBody2D.gravityScale,
                rigidBody2D.collisionDetectionMode,
                rigidBody2D.sleepMode,
                rigidBody2D.interpolation,
                rigidBody2D.constraints,
            });
        }
        
        public static void DeserializeFromJObject(this Rigidbody2D rigidBody2D, JObject jObject)
        {
            rigidBody2D.bodyType = jObject["bodyType"].ToObject<RigidbodyType2D>();
            rigidBody2D.simulated = jObject["simulated"].ToObject<bool>();
            rigidBody2D.useAutoMass = jObject["useAutoMass"].ToObject<bool>();
            rigidBody2D.mass = jObject["mass"].ToObject<float>();
            rigidBody2D.drag = jObject["drag"].ToObject<float>();
            rigidBody2D.angularDrag = jObject["angularDrag"].ToObject<float>();
            rigidBody2D.gravityScale = jObject["gravityScale"].ToObject<float>();
            rigidBody2D.collisionDetectionMode = jObject["collisionDetectionMode"].ToObject<CollisionDetectionMode2D>();
            rigidBody2D.sleepMode = jObject["sleepMode"].ToObject<RigidbodySleepMode2D>();
            rigidBody2D.interpolation = jObject["interpolation"].ToObject<RigidbodyInterpolation2D>();
            rigidBody2D.constraints = jObject["constraints"].ToObject<RigidbodyConstraints2D>();
        }

        public static JObject SerializeToJObject(this BoxCollider2D boxCollider2D)
        {
            return JObject.FromObject(new
            {
                boxCollider2D.isTrigger,
                boxCollider2D.usedByEffector,
                boxCollider2D.usedByComposite,
                boxCollider2D.autoTiling,
                offset = new
                {
                    boxCollider2D.offset.x,
                    boxCollider2D.offset.y
                },
                size = new{
                    boxCollider2D.size.x,
                    boxCollider2D.size.y
                },
                boxCollider2D.edgeRadius
            });
        }
        public static void DeserializeFromJObject(this BoxCollider2D boxCollider2D, JObject jObject)
        {
            boxCollider2D.isTrigger = jObject["isTrigger"].ToObject<bool>();
            boxCollider2D.usedByEffector = jObject["usedByEffector"].ToObject<bool>();
            boxCollider2D.usedByComposite = jObject["usedByComposite"].ToObject<bool>();
            boxCollider2D.autoTiling = jObject["autoTiling"].ToObject<bool>();
            boxCollider2D.offset = jObject["offset"].ToObject<Vector2>();
            boxCollider2D.size = jObject["size"].ToObject<Vector2>();
            boxCollider2D.edgeRadius = jObject["edgeRadius"].ToObject<float>();
        }


    }
    
    
}