#version 400 core

in vec3 vPosition;
in vec3 vColor;  

out vec3 fs_vColor;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(void) {
	vec4 pos = projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition, 1.0);
	gl_PointSize = (1.0f - pos.z/pos.w) * 128.0f;
	gl_Position = pos;
	fs_vColor = vColor;
}