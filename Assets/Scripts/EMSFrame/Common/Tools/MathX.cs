//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace UnityFrame{
	public static class MathX {

        /*
			获取字符串字符长度,一个中文算2个字符,其余的算1个
		 */
        public static int UF_GetStringByteLenUtf8(string param)
        {
            int len = 0;
            if (!string.IsNullOrEmpty(param))
            {
                char[] _strArr = param.ToCharArray();
                foreach (var item in _strArr)
                {
                    byte[] _byte = System.Text.Encoding.UTF8.GetBytes(item.ToString());
                    if (_byte.Length > 2)
                    {
                        len += 2;
                    }
                    else
                    {
                        len++;
                    }
                }
            }
            return len;
        }

		public static float UF_Distance(Vector3 _from,Vector3 _to){
			float _dx = _from.x - _to.x;
			float _dz = _from.z - _to.z;
			float _dy = _from.y - _to.y;
				return Mathf.Sqrt((_dx *_dx) + (_dy * _dy)+(_dz * _dz));
		}


		/// <summary>
		/// 水平距离
		/// </summary>
		/// <returns>The of horizontal.</returns>
		/// <param name="_from">From.</param>
		/// <param name="_to">To.</param>
		public static float UF_DistanceOfHorizontal(Vector3 _from,Vector3 _to){
			float _dx = _from.x - _to.x;
			float _dz = _from.z - _to.z;
			return Mathf.Sqrt((_dx *_dx) + (_dz * _dz));
		}

		/// <summary>
		/// 垂直距离
		/// </summary>
		/// <returns>The of vertical.</returns>
		/// <param name="_from">From.</param>
		/// <param name="_to">To.</param>
		public static float UF_DistanceOfVertical(Vector3 _from,Vector3 _to){
			float _dy = _from.y - _to.y;
			return Mathf.Sqrt(_dy * _dy);
		}

	    /// <summary>
	    /// 距离平方
	    /// </summary>
	    /// <returns>The square.</returns>
	    /// <param name="_from">From.</param>
	    /// <param name="_to">To.</param>
		public static float UF_DistanceSquare(Vector3 _from,Vector3 _to){
			float _dx = _from.x - _to.x;
			float _dz = _from.z - _to.z;
			float _dy = _from.y - _to.y;
				return (_dx *_dx) + (_dy * _dy)+(_dz * _dz);
		}

		/// <summary>
		/// 两点间插值
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="_">.</param>
		/// <param name="_to">To.</param>
		/// <param name="k">K.</param>
		public static Vector3 UF_InterPoint(Vector3 _,Vector3 _to,float k){
			float dist = Vector3.Distance(_,_to);
			if(dist < k){
				return _;
			}
			else{
				return (_ - _to).normalized * k + _to;
			}
		}

		/// <summary>
		/// 把一个角度范围化为0-360
		/// </summary>
		/// <returns>The eule angle abs.</returns>
		/// <param name="value">Value.</param>
		public static float UF_NormalizeAngle(float value){
			value = value % 360;
			value = (value < 0) ? (value + 360) : value;
			return value;
		}

		/// <summary>
		/// 计算两个欧拉角夹角差值
		/// </summary>
		/// <returns>The gap.</returns>
		/// <param name="_eularA">Eular a.</param>
		/// <param name="_eularB">Eular b.</param>
		public static float UF_EulerGap(Vector3 _eularA,Vector3 _eularB){
			_eularA.x = UF_NormalizeAngle (_eularA.x);
			_eularA.y = UF_NormalizeAngle (_eularA.y);
			_eularA.z = UF_NormalizeAngle (_eularA.z);

			_eularB.x = UF_NormalizeAngle (_eularB.x);
			_eularB.y = UF_NormalizeAngle (_eularB.y);
			_eularB.z = UF_NormalizeAngle (_eularB.z);

			Vector3 ret = _eularA - _eularB;
			return ret.x + ret.y + ret.z;
		}

		/// <summary>
		/// 求出欧拉角
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="">.</param>
		/// <param name="to">To.</param>
		public static Vector3 UF_EulerAngle(Vector3 from,Vector3 to){
			
				return UF_EulerAngle(to - from);
		}

		public static Vector3 UF_EulerAngle(Vector3 forward){
			if(forward == Vector3.zero)
				return Vector3.zero;
			return Quaternion.LookRotation(forward).eulerAngles;
		}

		/// <summary>
		/// 水平欧拉角
		/// </summary>
		/// <returns>The eular.</returns>
		/// <param name="_from">From.</param>
		/// <param name="_to">To.</param>
		public static Vector3 UF_HorizontalEular(Vector3 _from,Vector3 _to){
			return new Vector3(0,-Vector3.Angle(new Vector3(_to.x - _from.x,0,_to.z - _from.z),Vector3.forward),0);
		}

		/// <summary>
		/// 两个向量夹角
		/// </summary>
		/// <returns>The gap.</returns>
		/// <param name="_from">From.</param>
		/// <param name="_to">To.</param>
		/// <param name="_direction">Direction.</param>
		public static float UF_IncludedAngle(Vector3 _directionA,Vector3 _directionB){
			return Vector3.Angle(_directionA,_directionB);
		}


		public static Vector3 UF_Foward(Vector3 _from,Vector3 _to){
			Vector3 _direction = _to - _from;
			return _direction.normalized;
		}



		public static bool UF_CheckInRect(Vector2 _pos,Rect _rect){
			if(_pos.x > _rect.x &&
				_pos.x < _rect.x + _rect.width &&
				_pos.y > _rect.y &&
				_pos.y < _rect.y + _rect.height){

				return true;
			}
			else return false;
		}

		public static bool UF_CheckInCube(Vector3 _pos,Vector3 minDiagonalA,Vector3 maxDiagonalB){
			if(_pos.x > minDiagonalA.x &&
				_pos.x < minDiagonalA.x &&
				_pos.y > minDiagonalA.y &&
				_pos.y < maxDiagonalB.y &&
				_pos.z > minDiagonalA.z &&
				_pos.z < maxDiagonalB.z ){
				return true;
			}

			else return false;
		}


		public static bool UF_VectorEquals(Vector2 a,Vector2 b){
			return (a.x == b.x && a.y == b.y);
		}

		public static bool UF_VectorEquals(Vector3 a,Vector3 b){
			return (a.x == b.x && a.y == b.y && a.z == b.z);
		}

		/// <summary>
		/// 向量缩放大小
		/// </summary>
		public static float UF_VectorScaleSize(Vector3 scale){
			return (scale.x + scale.y + scale.x) / 3.0f;
		}


        //泛化角度值
        public static float UF_NormalAngle(float angle) {
            float absAngle = Mathf.Abs(angle);
            if (absAngle < 0.000001f)
                return 0;
            float side = angle > 0 ? 1 : -1;
            float nval = (absAngle % 360.0f) * side;
            if (nval < 0)
                nval += 360.0f;
            return nval;
        }

        public static Vector3 UF_RotateForward(Vector3 forward,float deg) {
            return Quaternion.Euler(0, deg, 0) * forward;
        }


        //计算两角度值的泛化角度差
        public static float UF_ClampNormalAngle(float angleA, float angleB) {
            return Mathf.Abs(UF_NormalAngle(angleA) - UF_NormalAngle(angleB));
        }


		public static Vector3 UF_SmoothDamp3(Vector3 current,Vector3 target,ref Vector3 vel,float smooth){
			Vector3 ret = current;
			ret.x = Mathf.SmoothDamp(current.x,target.x,ref vel.x,smooth);
			ret.y = Mathf.SmoothDamp(current.y,target.y,ref vel.y,smooth);
			ret.z = Mathf.SmoothDamp(current.z,target.z,ref vel.z,smooth);
			return ret;
		}


		public static Vector3 UF_SmoothAngleDamp3(Vector3 current,Vector3 target,ref Vector3 vel,float smooth){
			Vector3 ret = current;
			ret.x = Mathf.SmoothDampAngle(current.x,target.x,ref vel.x,smooth);
			ret.y = Mathf.SmoothDampAngle(current.y,target.y,ref vel.y,smooth);
			ret.z = Mathf.SmoothDampAngle(current.z,target.z,ref vel.z,smooth);
			return ret;
		}


		public static Vector3 UF_RandPointInRad(Vector3 position,Vector3 forward,float rad,float radius,float minRadius){
			float _radius = Random.Range (minRadius, radius);
			float hf_rad = rad / 2.0f;
			Vector3 newForward = Quaternion.Euler (0,Random.Range (-hf_rad, hf_rad),0) * forward ;
			return newForward.normalized * _radius+ position;
		}

		public static Vector3 UF_RotateForward(Vector3 forward,Vector3 euler){
			return (Quaternion.Euler (euler) * forward.normalized).normalized;
		}

        public static Vector3 UF_DegForward(Vector3 forward,float deg) {
            return Quaternion.Euler(0, deg, 0) * forward;
        }

        //面向直线点
        public static Vector3 UF_DirectPoint(Vector3 source,Vector3 target,float distance){
			return (source - target).normalized * distance + target;
		}

        public static Vector2 UF_MulVector2(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public static Vector3 UF_MulVector3(Vector3 a,Vector3 b) {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector4 UF_MulVector4(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z,a.w * b.w);
        }


        public static uint UF_GetHashCode(string str)
        {
            byte[] result = Encoding.UTF8.GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return System.BitConverter.ToUInt32(output, 0);
        }


        public static int UF_BitAdd(int val,int bit)
        {
            return val | bit;
        }

        public static int UF_BitSub(int val,int bit)
        {
            if ((val & bit) > 0)
                return val ^ bit;
            else
                return val;
        }


        public static Color UF_GetColor(int v) {
            return GHelper.UF_IntToColor(v);
        }

        public static Color UF_GetColor(string v)
        {
            return GHelper.UF_ParseStringToColor(v);
        }


    }


}