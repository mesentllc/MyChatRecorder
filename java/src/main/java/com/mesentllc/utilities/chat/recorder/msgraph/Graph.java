package com.mesentllc.utilities.chat.recorder.msgraph;

import com.microsoft.graph.logger.DefaultLogger;
import com.microsoft.graph.logger.LoggerLevel;
import com.microsoft.graph.models.extensions.Event;
import com.microsoft.graph.models.extensions.IGraphServiceClient;
import com.microsoft.graph.options.Option;
import com.microsoft.graph.options.QueryOption;
import com.microsoft.graph.requests.extensions.GraphServiceClient;
import com.microsoft.graph.requests.extensions.IEventCollectionPage;
import com.microsoft.graph.requests.extensions.IEventCollectionRequest;
import org.apache.commons.lang3.builder.ReflectionToStringBuilder;
import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;

public class Graph {
	private static final Log log = LogFactory.getLog(Graph.class);
	private static final List<Option> filterOptions = new ArrayList<>();
	private static final String FOUND = "Found ";
	private IGraphServiceClient client;

	public Graph(ZonedDateTime startingSunday) {
		String date = startingSunday.format(DateTimeFormatter.ISO_DATE).substring(0, 10);
		filterOptions.add(new QueryOption("startDateTime", date + "T00:00:00.0000000"));
		date = startingSunday.plusDays(6).format(DateTimeFormatter.ISO_DATE).substring(0, 10);
		filterOptions.add(new QueryOption("endDateTime",  date + "T23:59:59.9999999"));
	}

	public void resetGraphClient(String accessToken) {
		SimpleAuthProvider authProvider = new SimpleAuthProvider(accessToken);
		DefaultLogger logger = new DefaultLogger();
		logger.setLoggingLevel(LoggerLevel.ERROR);
		client = GraphServiceClient.builder().authenticationProvider(authProvider).logger(logger).buildClient();
		log.debug("Graph Client: " + new ReflectionToStringBuilder(client).toString());
	}

	public List<Event> getMyChats() {
		List<Event> eventList = new ArrayList<>();
		log.debug("Attempting to read chats.");
		IEventCollectionRequest request = client.me().chats().buildRequest(filterOptions);
		while (true) {
			IEventCollectionPage events = request.get();
			eventList.addAll(events.getCurrentPage());
			if (events.getNextPage() == null) {
				break;
			}
			request = events.getNextPage().buildRequest();
		}
		log.debug(FOUND + eventList.size() + " calendar events.");
		return eventList;
	}
}
