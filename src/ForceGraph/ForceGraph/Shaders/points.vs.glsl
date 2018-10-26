#version 400 core

layout(location=0) in vec3 vPosition;
layout(location=1) in vec3 vVelocity;
layout(location=2) in vec3 vColor;
layout(location=3) in ivec3 vIndex;

out vec3 ft_vPosition;
out vec3 ft_vVelocity;
out vec3 ft_vColor;
out ivec3 ft_vIndex;

out vec3 fs_vColor;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform int nPoints;

uniform sampler2D depTex;
uniform samplerBuffer tbo;

uniform float timeStep = .08f;

void main(void) {
	int index = vIndex.r;
	vec3 a = vec3(0.0f, .0f, .0f);
	for(int i = 0; i < nPoints; ++i)
	{
		float dep = texelFetch(depTex, ivec2(index,i) ,0).r;
		if(dep != .0f)
		{
			vec3 otherPosition = texelFetch(tbo, i * 4).xyz;
			vec3 diff = vPosition - otherPosition;
			float len = length(diff);

			if (len != 0.0)
			{
				//repulse
				vec3 n = diff / len;
				float force = .05f/len;
				//a = a + vec3(force * n.x, force * n.y, .0f);
			
				//spring
				force = .5f * (len - .5f);
				a = a - vec3(force * n.x, force * n.y, .0f);
			}
		}

	}
	//friction
	a = a - .5f * vVelocity/timeStep;

	vec3 newVelocity = vVelocity + a * timeStep;
	vec3 newPos = vPosition + newVelocity * timeStep;

	vec4 pos = projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition, 1.0);
	
	gl_PointSize = (1.0f - pos.z/pos.w) * 128.0f;
	gl_Position = pos;
	
	fs_vColor = vColor;
	//fs_vColor = vec3(-diff.y);

	ft_vPosition = newPos;
	ft_vVelocity = newVelocity;
	ft_vColor = vColor;
	ft_vIndex = vIndex;
}