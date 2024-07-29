#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 uv_f;

void main()
{
    mat4 vm = view * model;
    gl_Position = projection * vm * vec4(position, 1.0);
	uv_f = uv;
}