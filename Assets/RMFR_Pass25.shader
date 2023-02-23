Shader "RMFR_Pass25"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MidTex("Texture", 2D) = "white" {}
		_NoiseTex("_NoiseTex" , 2D) = "white" {}
		_eyeX("_eyeX", float) = 0.5
		_eyeY("_eyeY", float) = 0.5
		_scaleRatio("_scaleRatio", float) = 2.0
		_fx("_fx", float) = 1.0
		_fy("_fy", float) = 1.0
		_iApplyLogMap2("_iApplyRFRMap2", int) = 1
		_SquelchedGridMappingBeta("_SquelchedGridMappingBeta", float ) = 0.0
		_MappingStrategy("_MappingStrategy" , int ) = 0
		
		_bSampleDensityWithNoise("_bSampleDensityWithNoise" , float ) = 0
		_DebugMode("_DebugMode", int ) = 0

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
			sampler2D _NoiseTex;
			uniform float _eyeX;
			uniform float _eyeY;
			uniform float _scaleRatio;
			uniform float _kernel;
			uniform float _fx;
			uniform float _fy;
			uniform int _iApplyRFRMap2;
			uniform float _SquelchedGridMappingBeta;
			uniform int _MappingStrategy;
			uniform float _bSampleDensityWithNoise;
			uniform int _DebugMode;

			fixed4 frag(v2f i) : SV_Target
			{
				if (_iApplyRFRMap2 < 0.5)
					return tex2D(_MidTex, i.uv);

				float2 cursorPos = float2(_eyeX, _eyeY); //0-1 -> -1,1 (0,0)
				float2 tc = (i.uv - cursorPos);






				float u,v;
				float2 tt = tc;
				tt /= sqrt(2);
				tt = tt * 2 ;      //  [ -1 , 1 ]
				float xx = pow( tt.x , 2 );
				float yy = pow( tt.y , 2 );



				
				// elliptical Grid Mapping
				// Disc to Square
				if( _MappingStrategy == 1 )
				{
					if(2 + xx - yy + 2 * sqrt(2) * tt.x < 0 ) return fixed4(0,1,0,1);
					if(2 + xx - yy - 2 * sqrt(2) * tt.x < 0 ) return fixed4(0,1,0,1);
					if(2 - xx + yy + 2 * sqrt(2) * tt.y < 0 ) return fixed4(0,1,0,1);
					if(2 - xx + yy - 2 * sqrt(2) * tt.y < 0 ) return fixed4(0,1,0,1);
						
					u = 0.5 * sqrt( 2 + xx - yy + 2 * sqrt(2) * tt.x ) - 0.5 * sqrt( 2 + xx - yy - 2 * sqrt(2) * tt.x ) ;
					v = 0.5 * sqrt( 2 - xx + yy + 2 * sqrt(2) * tt.y ) - 0.5 * sqrt( 2 - xx + yy - 2 * sqrt(2) * tt.y ) ;
					
				}

				// Squelched Grid Open Mapping
				// Disc to Square
				if(_MappingStrategy == 2 )
				{
					u = tt.x / sqrt( 1- yy ) ;
					v = tt.y / sqrt( 1- xx ) ;
				}


				// Blended E-Grid mapping
				if( _MappingStrategy == 3 )
				{

					float beta = _SquelchedGridMappingBeta;
					float ax = beta + 1 + beta*xx - yy ;
					float ay = beta + 1 + beta*yy - xx ;
					float c = 4 * beta * ( beta + 1 ) ;
					u = sign( tt.x ) / sqrt( 2 * beta ) * sqrt( ax - sqrt( ax*ax - c*xx )); 
					v = sign( tt.y ) / sqrt( 2 * beta ) * sqrt( ay - sqrt( ay*ay - c*yy )); 
						
				}


				// FG-Spuircular Mapping with & without S and K
				// Disc to Square Mapping
				if( _MappingStrategy == 4 )
				{

					float S = 1.0f;
					float SS = S * S;
					if( tt.x == 0 || tt.y == 0 ) discard;
						
					float temp = ( xx + yy ) * ( xx + yy - 4 * SS * xx * yy ) ;   // float temp = ( xx + yy ) * ( xx + yy - 4 * SS * xx * yy / ( xx + yy )) ;
					if( temp < 0 ) return fixed4(0,1,0,1);
					temp = sqrt( temp );
					temp = xx + yy - temp ;
					if( temp < 0 ) return fixed4(0,1,0,1);
					temp = sqrt( temp );
					temp = sign( tt.x * tt.y ) / S / sqrt(2) * temp ;     // temp = sign( x * y ) / S / sqrt(2) * temp * sqrt( xx + yy ) ;
						
					u = temp / tt.y;
					v = temp / tt.x;
					
				}


				// 2-Squircular mapping
				// Disc to Square mapping
				if( _MappingStrategy == 5 )
				{
					float var1 = sqrt( 1 - sqrt( 1 - 4 * xx * yy ) );
					var1 = var1 * sign( tt.x * tt.y ) / ( sqrt(2) );
					u = var1 / tt.y ;
					v = var1 / tt.x ;
				}
				

				// hyperbolic
				if( _MappingStrategy == 8 )
				{
					float a = 0.5;
					float aa = a * a;

					float bb = aa  / ( 1 - aa );
					if( abs(tt.x) > abs(tt.y) )
					{
						u = sign(tt.x) * sqrt( xx/aa - yy/bb );
						v = tt.y;
					}
					if( abs(tt.y) >= abs(tt.x) )
					{
						v = sign(tt.y) * sqrt( yy/aa - xx/bb );
						u = tt.x;
					}
				}

				
				// cornerific tapered2 mapping
				// Disc to Square
				if( _MappingStrategy == 9 )
				{					
					float var0 = xx + yy ;
					float var1 = 2 - var0 ;
					float var2 = 4 * xx * yy * var1;
					var2 = var0 - var2 ;
					var2 = var0 * var2 ;
					var2 = sqrt(var2) ;
					var2 = var0 - var2 ;
					var2 /= 2 * var1 ;
					var2 = sqrt(var2) ;
					var2 *= sign( tt.x * tt.y );
					
					u = var2 / tt.y;
					v = var2 / tt.x;
				}

				
				// Non-axial 2-pinch mapping
				// Disc to Square
				if( _MappingStrategy == 10 )
				{
					
					float var0 = xx + yy ;
					float var1 = var0 - 4*xx*yy;
					var1 *= var0;
					var1 = pow( var1 , 0.5 );
					var1 = var0 - 2 * xx * yy - var1;
					var1 = pow( var1 , 0.25 );
					var1 *= sign( tt.x * tt.y ) / (  pow( 2 , 0.25 ) );
					
					u = var1 / tt.y ;
					v = var1 / tt.x ;
				}


					
				// --------------------------
				
				if( _MappingStrategy > 0 )
				{
					u/=2;
					v/=2;

					tc = fixed2(u,v);  //  [ -0.5 , 0.5 ] 
					
				}

				// --------------------------




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


				if( _DebugMode == 1 )
				{
					
					float res = 800 / _scaleRatio ;
					float OriginRes = 800;
				
				
				
					pq *= res ;  // pos of RectMapping Tex   [ 0 , 800 / Scale ]   // pq = fixed2( col.x ,col.y );        /// value( pos in Original Tex ) in the RectMappingTex, not the pos of it.
				   
					fixed2 point1 = fixed2( floor(pq.x) / res , floor(pq.y) / res ) ;
					fixed2 point3 = fixed2( ceil(pq.x)  / res , floor(pq.y) / res ) ;   
					fixed2 point2 = fixed2( ceil(pq.x)  / res , ceil(pq.y)  / res ) ;
					fixed2 point4 = fixed2( floor(pq.x) / res , ceil(pq.y)  / res ) ;

					// point1 += frac( sin( x + y ) * 10000.0 ) * 0.01 * ( _bSampleDensityWithNoise - 0.5 )  ;
					// point2 += frac( sin( x + y ) * 10000.0 ) * 0.01 * ( _bSampleDensityWithNoise - 0.5 )  ; 
					// point3 += frac( sin( x + y ) * 10000.0 ) * 0.01 * ( _bSampleDensityWithNoise - 0.5 )  ;
					// point4 += frac( sin( x + y ) * 10000.0 ) * 0.01 * ( _bSampleDensityWithNoise - 0.5 )  ;
					//
					point1 += ( tex2D( _NoiseTex , fixed2(x,y)).x - 0.5 ) * 0.01 * ( _bSampleDensityWithNoise ) ; 
					point2 += ( tex2D( _NoiseTex , fixed2(x,y)).x - 0.5 ) * 0.01 * ( _bSampleDensityWithNoise ) ; 
					point3 += ( tex2D( _NoiseTex , fixed2(x,y)).x - 0.5 ) * 0.01 * ( _bSampleDensityWithNoise ) ; 
					point4 += ( tex2D( _NoiseTex , fixed2(x,y)).x - 0.5 ) * 0.01 * ( _bSampleDensityWithNoise ) ; 

					
					if( point1.x < 0 || point1.x > 1 || point1.y < 0 || point1.y > 1 ) return fixed4(0,0,0,1);
					if( point2.x < 0 || point2.x > 1 || point2.y < 0 || point2.y > 1 ) return fixed4(0,0,0,1);
					if( point3.x < 0 || point3.x > 1 || point3.y < 0 || point3.y > 1 ) return fixed4(0,0,0,1);
					if( point4.x < 0 || point4.x > 1 || point4.y < 0 || point4.y > 1 ) return fixed4(0,0,0,1);
					
					point1 = tex2D( _MidTex, point1 ).xy ;
					point2 = tex2D( _MidTex, point2 ).xy ;
					point3 = tex2D( _MidTex, point3 ).xy ;
					point4 = tex2D( _MidTex, point4 ).xy ;
				
				
					if( point1.x < 0 || point1.x > 1 || point1.y < 0 || point1.y > 1 ) return fixed4(0,0,0,1);
					if( point2.x < 0 || point2.x > 1 || point2.y < 0 || point2.y > 1 ) return fixed4(0,0,0,1);
					if( point3.x < 0 || point3.x > 1 || point3.y < 0 || point3.y > 1 ) return fixed4(0,0,0,1);
					if( point4.x < 0 || point4.x > 1 || point4.y < 0 || point4.y > 1 ) return fixed4(0,0,0,1);
				
					point1 *= OriginRes ;
					point2 *= OriginRes ;
					point3 *= OriginRes ;
					point4 *= OriginRes ;
				
					float s1 = 0.5 * ( point1.x * point2.y - point1.y * point2.x + point2.x * point3.y - point2.y * point3.x + point3.x * point1.y - point3.y * point1.x);
					float s2 = 0.5 * ( point1.x * point2.y - point1.y * point2.x + point2.x * point4.y - point2.y * point4.x + point4.x * point1.y - point4.y * point1.x);

					float density;
					if( _MappingStrategy > 0 )
					{
						density = 1 / ( ( abs(s1) + abs(s2) ) * 0.78 );
						
					}
					else if (_MappingStrategy == 0 )
					{
						density = 1 / ( ( abs(s1) + abs(s2) )  );
						
					}
					return fixed4( density ,density,density ,1);
				}


				return col;
			}
		ENDCG
		}
	}
}