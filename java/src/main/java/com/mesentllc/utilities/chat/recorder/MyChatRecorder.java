package com.mesentllc.utilities.chat.recorder;

import com.mesentllc.utilities.chat.recorder.msgraph.Authentication;
import com.mesentllc.utilities.chat.recorder.msgraph.Graph;
import com.mesentllc.utilities.chat.recorder.utils.ClassPathResourceUtil;
import com.microsoft.graph.models.extensions.Event;
import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import java.io.IOException;
import java.time.DayOfWeek;
import java.time.Duration;
import java.time.LocalDateTime;
import java.time.ZonedDateTime;
import java.time.temporal.ChronoUnit;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.TreeMap;

public class MyChatRecorder {
	private static final Log log = LogFactory.getLog(MyChatRecorder.class);
	private static final Properties oAuthProperties = new Properties();

	private MyChatRecorder() {
		System.setProperty("https.proxyHost","internet.proxy.fedex.com");
		System.setProperty("https.proxyPort","3128");
		setOAuth();
		Authentication.initialize(oAuthProperties.getProperty("app.id"));
	}

	private void setOAuth() {
		try {
			oAuthProperties.load(ClassPathResourceUtil.getStream("oAuth.properties"));
		}
		catch (IOException e) {
			log.error("Unable to read OAuth configuration. Make sure you have a properly formatted oAuth.properties file. See README for details.");
			System.exit(1);
		}
	}

	private void process() {
		ZonedDateTime lastSunday = ZonedDateTime.now();
		if (lastSunday.getDayOfWeek().getValue() < DayOfWeek.SUNDAY.getValue()) {
			lastSunday = lastSunday.minusDays(lastSunday.getDayOfWeek().getValue());
		}
		Graph graph = new Graph(lastSunday);
		Map<Integer, List<Event>> eventMap = new TreeMap<>();
		graph.resetGraphClient(Authentication.getAccessToken(oAuthProperties.getProperty("app.scopes").split(",")));
		List<Event> events = graph.getMyChats();
		for (Event event : events) {
			Integer dow = LocalDateTime.parse(event.start.dateTime).toLocalDate().getDayOfWeek().getValue();
			if (dow == 7) {
				dow = 0;
			}
			if (!eventMap.containsKey(dow)) {
				eventMap.put(dow, new ArrayList<>());
			}
			eventMap.get(dow).add(event);
		}
		for (Integer dow : eventMap.keySet()) {
			for (Event event : eventMap.get(dow)) {
				LocalDateTime start = LocalDateTime.parse(event.start.dateTime);
				LocalDateTime end = LocalDateTime.parse(event.end.dateTime);
				Duration duration = Duration.between(start, end);
				long hours = duration.toHours();
				duration = duration.minus(hours, ChronoUnit.HOURS);
				log.info(String.format("Day: %s, Subject: %s, Duration: %02d:%02d", start.toLocalDate().getDayOfWeek(), event.subject, hours, duration.toMinutes()));
			}
		}
	}

	public static void main(String[] args) {
		MyChatRecorder myChatRecorder = new MyChatRecorder();
		myChatRecorder.process();
	}
}
