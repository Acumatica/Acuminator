#version 400 core
in vec3 fs_vColor;
out vec4 frag_vColor;

uniform vec3 vLightDir;

void main(void) {
    vec3 normal;
    normal.xy = gl_PointCoord* 2.0 - vec2(1.0);    
    float mag = dot(normal.xy, normal.xy);
    if (mag > 1.0) discard;
    normal.z = sqrt(1.0-mag);

    float diffuse = max(0.0, dot(vLightDir, normal));

	vec4 color1 = vec4(fs_vColor, 1.0);
    const vec4 color2 = vec4(0.9, 0.7, 1.0, 0.0);
	vec4 color = mix(color1, color2, smoothstep(.85f, 1.0f, mag));
    frag_vColor = color1 * diffuse;
}
