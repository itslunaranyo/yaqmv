#version 330 core
in float shade_f;

out vec4 FragColor;

void main()
{
    FragColor = vec4(1,1,1,1) * shade_f;
}