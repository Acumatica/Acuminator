#version 400 core

layout(location=0) in vec3 vPosition;
layout(location=1) in vec3 vColor;
layout(location=2) in vec3 vVelocity;

out vec3 ft_vPosition;
out vec3 ft_vVelocity;

out vec3 fs_vColor;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

uniform float timeStep = .02f;

void main(void) {
	vec3 a = vec3(.0f, -.3f, .0f);
	vec3 newVelocity = vVelocity + a * timeStep;
	vec3 newPos = vPosition + newVelocity * timeStep;

	vec4 pos = projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition, 1.0);
	gl_PointSize = (1.0f - pos.z/pos.w) * 128.0f;
	gl_Position = pos;
	fs_vColor = vColor;

	ft_vPosition = newPos;
	ft_vVelocity = newVelocity;
}