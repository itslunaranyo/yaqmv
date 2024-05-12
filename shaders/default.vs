#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 color;

void main()
{
    mat4 vm = view * model;
    gl_Position = projection * vm * vec4(position, 1.0);
    vec4 normal_vs = transpose(inverse(vm)) * vec4(normal, 1.0);
    color = normal_vs.xyz * 0.5 + 0.5;
}