//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

#if UNITY_EDITOR

using UnityEngine;
using System.Collections;


namespace UnityFrame{
	public class GizmosTools{

		public static void DrawCircle(Vector3 center,float radius){
			float rx = 0;
			float ry = 0;
			Vector3 from = new Vector3(0,0,radius) + center;
			Vector3 to = Vector3.zero + center;
			for(int k = 0;k <= 360;k++){
				rx =radius * Mathf.Sin(Mathf.Deg2Rad * k);
				ry =radius * Mathf.Cos(Mathf.Deg2Rad * k);
				to = center + new Vector3(rx,0,ry);
				Gizmos.DrawLine(from,to);
				from = to;
			}
		}

        public static void DrawCircle2D(Vector3 center, float radius)
        {
            float rx = 0;
            float ry = 0;
            Vector3 from = new Vector3(0, radius/2, 0) + center;
            Vector3 to = Vector3.zero + center;
            for (int k = 0; k <= 360; k++)
            {
                rx = radius * Mathf.Sin(Mathf.Deg2Rad * k);
                ry = radius * Mathf.Cos(Mathf.Deg2Rad * k);
                to = center + new Vector3(rx, ry/2,0);
                Gizmos.DrawLine(from, to);
                from = to;
            }
        }

        public static void DrawCylinder(Vector3 center,float radius,float height){
			Vector3 vup = center + height*Vector3.up/2;
			Vector3 vdown = center - height * Vector3.up /2;

			Vector3 vforward = Vector3.forward * radius;
			Vector3 vright = Vector3.right * radius;

			DrawCircle(vup,radius);
			DrawCircle(vdown,radius);

			Gizmos.DrawLine(vforward + vup,vforward + vdown);
			Gizmos.DrawLine(-vforward + vup,-vforward + vdown);
			Gizmos.DrawLine(vright + vup,vright + vdown);
			Gizmos.DrawLine(-vright + vup,-vright + vdown);
		}



        public static void DrawBox2D(Vector3 center, float width, float height) {
            Gizmos.DrawWireCube(center,new Vector3(width, height));
        }

        public static void DrawCapsule2D(Vector3 center, float radius, float height)
        {
            height -= radius * 2;
            Vector3 vup = center + height * Vector3.up / 2;
            Vector3 vdown = center - height * Vector3.up / 2;
            float rx = 0;
            float ry = 0;
            var from = new Vector3(radius, 0, 0) + vup;
            var to = Vector3.zero + vup;
            for (int k = 0; k <= 361; k++)
            {
                rx = radius * Mathf.Sin(Mathf.Deg2Rad * k);
                ry = radius * Mathf.Cos(Mathf.Deg2Rad * k);
                if (k > 180 && k < 361)
                {
                    to = vdown + new Vector3(ry, rx, 0);
                }
                else
                    to = vup + new Vector3(ry, rx, 0);
                Gizmos.DrawLine(from, to);
                from = to;
            }


        }

		public static void DrawCapsule(Vector3 center,float radius,float height){
			height -= radius*2;
			Vector3 vup = center + height*Vector3.up/2;
			Vector3 vdown = center - height * Vector3.up /2;
			//		Vector3 vforward = Vector3.forward * radius;
			//		Vector3 vright = Vector3.right * radius;
			DrawCircle(vup,radius);
			DrawCircle(vdown,radius);
			float rx = 0;
			float ry = 0;
			Vector3 from = new Vector3(0,0,radius) + vup;
			Vector3 to = Vector3.zero + vup;
			for(int k = 0;k <= 361;k++){
				rx =radius * Mathf.Sin(Mathf.Deg2Rad * k);
				ry =radius * Mathf.Cos(Mathf.Deg2Rad * k);
				if(k > 180 && k < 361){
					to = vdown + new Vector3(0,rx,ry);
				}
				else
					to = vup + new Vector3(0,rx,ry);
				Gizmos.DrawLine(from,to);
				from = to;
			}
			from = new Vector3(radius,0,0) + vup;
			to = Vector3.zero + vup;
			for(int k = 0;k <= 361;k++){
				rx =radius * Mathf.Sin(Mathf.Deg2Rad * k);
				ry =radius * Mathf.Cos(Mathf.Deg2Rad * k);
				if(k > 180 && k < 361){
					to = vdown + new Vector3(ry,rx,0);
				}
				else
					to = vup + new Vector3(ry,rx,0);
				Gizmos.DrawLine(from,to);
				from = to;
			}

		}


	}


}
#endif