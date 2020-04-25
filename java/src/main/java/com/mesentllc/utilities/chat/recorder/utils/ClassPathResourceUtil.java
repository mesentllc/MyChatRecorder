package com.mesentllc.utilities.chat.recorder.utils;

import org.springframework.core.io.ClassPathResource;

import java.io.IOException;
import java.io.InputStream;

public class ClassPathResourceUtil {
	public static synchronized InputStream getStream(String path) throws IOException {
		ClassPathResource classPathResource = new ClassPathResource(path);
		return classPathResource.getInputStream();
	}
}
