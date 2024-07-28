#version 330 core
in vec2 uv_f;

out vec4 FragColor;

uniform sampler2D texture0;

void main()
{
	FragColor = texture(texture0, uv_f);
}