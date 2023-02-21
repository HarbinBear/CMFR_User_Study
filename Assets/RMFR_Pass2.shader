Shader "Custom/RMFR_Pass2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MidTex("Texture", 2D) = "white" {}
		_NoiseTex("Texture", 2D) = "white" {}
		_eyeX("_eyeX", float) = 0.5
		_eyeY("_eyeY", float) = 0.5
		_scaleRatio("_scaleRatio", float) = 2.0
		_fx("_fx", float) = 1.0
		_fy("_fy", float) = 1.0
		_iApplyLogMap2("_iApplyRFRMap2", int) = 1
		_SquelchedGridMappingBeta("_SquelchedGridMappingBeta", float ) = 0.0
		_MappingStrategy("_MappingStrategy" , int ) = 0
		
		_bSampleDensityWithNoise("_bSampleDensityWithNoise" , int ) = 0

	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			sampler2D _MainTex;
			sampler2D _MidTex;
			uniform float _eyeX;
			uniform float _eyeY;
			uniform float _scaleRatio;
			uniform float _kernel;
			uniform float _fx;
			uniform float _fy;
			uniform int _iApplyRFRMap2;
			uniform float _SquelchedGridMappingBeta;
			uniform int _MappingStrategy;
			uniform int _bSampleDensityWithNoise;

			fixed4 frag(v2f i) : SV_Target
			{
				if (_iApplyRFRMap2 < 0.5)
					return tex2D(_MidTex, i.uv);

				float2 cursorPos = float2(_eyeX, _eyeY); //0-1 -> -1,1 (0,0)
				float2 tc = (i.uv - cursorPos);

				// ------------------------------------------------------
				float2 tt = tc;
				tt = tt * 2 ;      //  [ -1 , 1 ]
				float xx = pow( tt.x , 2 );
				float yy = pow( tt.y , 2 );

				if( _MappingStrategy == 2 )
				{
	//  Squelched Grid Open Mapping
	
					float b = 1.0;
					tc.x = tt.x * sqrt( ( 1 - b * yy ) / ( 1 - b * xx * yy ) );
					tc.y = tt.y * sqrt( ( 1 - b * xx ) / ( 1 - b * xx * yy ) );
					
				}
	
				if(_MappingStrategy == 1 )
				{
	//
	// Elliptical Grid Mapping
	// Square to Disc Mapping
	
					tc.x = tt.x * sqrt( 1 - yy / 2 );
					tc.y = tt.y * sqrt( 1 - xx / 2 );
					
				}
	
	
				if ( _MappingStrategy == 3 )
				{
	// Blended E-Grid mapping
	
					float beta = _SquelchedGridMappingBeta;
					float a = beta + 1 - beta * xx;	
					float b = beta + 1 - beta * yy;	
					tc.x = tt.x * sqrt( ( yy * b - a*b ) / ( xx*yy - a*b ) );
					tc.y = tt.y * sqrt( ( xx * b - a*b ) / ( xx*yy - a*b ) );
						
				}

				if( _MappingStrategy == 8 )
				{
					// hyperbolic
					float a = 0.8;
					float aa = a * a;
					float bb = aa  / ( 1 - aa );
					if( abs(tt.x) > abs(tt.y) )
					{
						tc.x = sign(tt.x) * sqrt( aa*xx + aa/bb*yy );
						tc.y = tt.y;
					}
					if( abs(tt.y) >= abs(tt.x) )
					{
						tc.y = sign(tt.y) * sqrt( aa*yy + aa/bb*xx );
						tc.x = tt.x;
					}
				}



				
	
				if( _MappingStrategy != 0 )
				{
					
					tc = ( tc ) / 2 ;    //  [ -0.5 , 0.5 ]
				}
				
				// -------------- -------------------------------------------
					
				float maxDxPos = 1.0 - cursorPos.x; // >= 0.5
				float maxDyPos = 1.0 - cursorPos.y;
				float maxDxNeg = cursorPos.x;
				float maxDyNeg = cursorPos.y;

				float norDxPos = _fx * maxDxPos / (_fx + maxDxPos);
				float norDyPos = _fy * maxDyPos / (_fy + maxDyPos);
				float norDxNeg = _fx * maxDxNeg / (_fx + maxDxNeg);
				float norDyNeg = _fy * maxDyNeg / (_fy + maxDyNeg);

				float x = 0.0;
				float y = 0.0;
				float2 pq = float2(0.0, 0.0);
				if (tc.x >= 0) {
					x = _fx * tc.x / (_fx + tc.x); //>0
					x = x / norDxPos;
					pq.x = x * maxDxPos + cursorPos.x;
				}
				else {
					x = _fx * tc.x / (_fx - tc.x); //<0
					x = x / norDxNeg;
					pq.x = x * maxDxNeg + cursorPos.x;
				}

				if (tc.y >= 0) {
					y = _fy * tc.y / (_fy + tc.y);
					y = y / norDyPos;
					pq.y = y * maxDyPos + cursorPos.y;
				}
				else {
					y = _fy * tc.y / (_fy - tc.y);
					y = y / norDyNeg;
					pq.y = y * maxDyNeg + cursorPos.y;
				}
				fixed4 col = tex2D(_MidTex, pq);
				// return fixed4( pq , 0 ,1);


				// ------------------------------------------------------------
				//
				// pq.x = pq.x * 2.0 - 1.0;
				// pq.y = pq.y * 2.0 - 1.0;
				//
				// tc = pq;
				// pq.x = tc.x * sqrt( ( 1 - pow( tc.y , 2 ) ) / ( 1 - pow( tc.x , 2 ) * pow( tc.y , 2 ) ) );
				// pq.y = tc.y * sqrt( ( 1 - pow( tc.x , 2 ) ) / ( 1 - pow( tc.x , 2 ) * pow( tc.y , 2 ) ) );
				//
				// pq = (pq + 1) / 2 ;
				//
				// return tex2D( _MidTex , pq ) ;

				// ------------------------------------------------------------

				// float u = pq.x / sqrt( 1- pow( pq.y , 2) ) ;
				// float v = pq.y / sqrt( 1- pow( pq.x , 2) ) ;
				//
				//
				// return tex2D(_MidTex, fixed2(u,v));

					
//				// ------------------------------------------------------------

				float res = 800 / _scaleRatio ;
				float OriginRes = 800;
				
				
				
				pq *= res ;  // pos of RectMapping Tex   [ 0 , 800 / Scale ]   // pq = fixed2( col.x ,col.y );        /// value( pos in Original Tex ) in the RectMappingTex, not the pos of it.
				   
				fixed2 point1 = fixed2( floor(pq.x) / res , floor(pq.y) / res ) + frac(sin( x + y )*10000.0) * 0.01 * _bSampleDensityWithNoise  ;
				fixed2 point3 = fixed2( ceil(pq.x)  / res , floor(pq.y) / res ) + frac(sin( x + y )*10000.0) * 0.01 * _bSampleDensityWithNoise  ;    
				fixed2 point2 = fixed2( ceil(pq.x)  / res , ceil(pq.y)  / res ) + frac(sin( x + y )*10000.0) * 0.01 * _bSampleDensityWithNoise   ;
				fixed2 point4 = fixed2( floor(pq.x) / res , ceil(pq.y)  / res ) + frac(sin( x + y )*10000.0) * 0.01 * _bSampleDensityWithNoise  ;
				
				if( point1.x < 0 || point1.x > 1 || point1.y < 0 || point1.y > 1 ) return fixed4(0,0,0,1);
				if( point2.x < 0 || point2.x > 1 || point2.y < 0 || point2.y > 1 ) return fixed4(0,0,0,1);
				if( point3.x < 0 || point3.x > 1 || point3.y < 0 || point3.y > 1 ) return fixed4(0,0,0,1);
				if( point4.x < 0 || point4.x > 1 || point4.y < 0 || point4.y > 1 ) return fixed4(0,0,0,1);
				
				point1 = tex2D( _MidTex, point1 ).xy ;
				point2 = tex2D( _MidTex, point2 ).xy ;
				point3 = tex2D( _MidTex, point3 ).xy ;
				point4 = tex2D( _MidTex, point4 ).xy ;
				
				point1 *= OriginRes ;
				point2 *= OriginRes ;
				point3 *= OriginRes ;
				point4 *= OriginRes ;
				
				float s1 = 0.5 * ( point1.x * point2.y - point1.y * point2.x + point2.x * point3.y - point2.y * point3.x + point3.x * point1.y - point3.y * point1.x);
				float s2 = 0.5 * ( point1.x * point2.y - point1.y * point2.x + point2.x * point4.y - point2.y * point4.x + point4.x * point1.y - point4.y * point1.x);
				
				float density = 1 / ( abs(s1) + abs(s2) );
				return fixed4( density ,density,density ,1);
				
				
				
				


					
					
				return col;
			}
			ENDCG
		}
	}
}
