#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec3 normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out float shade_f;
out vec2 uv_f;

void main()
{
    mat4 vm = view * model;
    gl_Position = projection * vm * vec4(position, 1.0);
    vec4 normal_vs = transpose(inverse(vm)) * vec4(normal, 1.0);
	shade_f = clamp(normal_vs.x * 0.5 + 0.75, 0, 1);
	uv_f = uv;
}