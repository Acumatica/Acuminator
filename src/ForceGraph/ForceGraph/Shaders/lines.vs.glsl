#version 400 core

layout(location=0) in ivec2 index;

out vec4 fs_vColor;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform int nPoints;

uniform sampler2D depTex;
uniform samplerBuffer tbo;

void main(void) {
	vec3 position = texelFetch(tbo, index.r * 4).xyz;
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(position, 1.0);
	fs_vColor = vec4(1.0f);
}