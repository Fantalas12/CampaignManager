import React, { useState, useEffect } from 'react';
import { useGetNotesByTag, useGetParticipatedCampaigns, useGetSessions } from '../hooks/useQueries';
import { Note, Campaign, Session } from '../backend';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { ArrowLeft, FileText, Calendar, MapPin, Tag } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';

interface TagNavigationViewProps {
  selectedTag: string;
  onBack: () => void;
  onNoteClick: (note: Note, session: Session, campaign: Campaign) => void;
}

interface NoteWithContext {
  note: Note;
  session: Session;
  campaign: Campaign;
}

export default function TagNavigationView({ selectedTag, onBack, onNoteClick }: TagNavigationViewProps) {
  const getNotesByTag = useGetNotesByTag();
  const { data: participatedCampaigns } = useGetParticipatedCampaigns();
  const getSessions = useGetSessions();
  const [notesWithContext, setNotesWithContext] = useState<NoteWithContext[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchNotesWithContext = async () => {
      if (!participatedCampaigns) return;
      
      setIsLoading(true);
      try {
        // Get all notes with the selected tag
        const taggedNotes = await getNotesByTag.mutateAsync(selectedTag);
        
        // Build a map of sessions and campaigns for context
        const sessionMap = new Map<string, Session>();
        const campaignMap = new Map<string, Campaign>();
        
        // Populate campaign map
        participatedCampaigns.forEach(campaign => {
          campaignMap.set(campaign.id, campaign);
        });
        
        // Get sessions for each campaign and populate session map
        for (const campaign of participatedCampaigns) {
          try {
            const sessions = await getSessions.mutateAsync(campaign.id);
            sessions.forEach(session => {
              sessionMap.set(session.id, session);
            });
          } catch (error) {
            // Continue if we can't access sessions for this campaign
            continue;
          }
        }
        
        // Build notes with context
        const notesWithContextData: NoteWithContext[] = [];
        for (const note of taggedNotes) {
          const session = sessionMap.get(note.sessionId);
          if (session) {
            const campaign = campaignMap.get(session.campaignId);
            if (campaign) {
              notesWithContextData.push({
                note,
                session,
                campaign
              });
            }
          }
        }
        
        setNotesWithContext(notesWithContextData);
      } catch (error) {
        toast.error('Failed to load notes for this tag');
        console.error('Error fetching notes by tag:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchNotesWithContext();
  }, [selectedTag, participatedCampaigns, getNotesByTag, getSessions]);

  const handleNoteClick = (noteWithContext: NoteWithContext) => {
    // For now, just show a toast since we can't navigate directly to notes from tags
    // The existing navigation system goes through campaigns -> sessions -> notes
    toast.info(`Note: ${noteWithContext.note.name} from ${noteWithContext.campaign.name} - ${noteWithContext.session.name}`);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center space-x-4">
          <Skeleton className="h-10 w-24 bg-white/10" />
          <Skeleton className="h-8 w-48 bg-white/10" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(6)].map((_, i) => (
            <Skeleton key={i} className="h-48 bg-white/10" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Button
          onClick={onBack}
          variant="outline"
          className="border-white/20 text-white hover:bg-white/10"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <div className="flex items-center space-x-3">
          <Tag className="h-8 w-8 text-purple-400" />
          <div>
            <h2 className="text-3xl font-bold text-white">Notes Tagged: {selectedTag}</h2>
            <p className="text-purple-200">
              {notesWithContext.length === 0 
                ? "No accessible notes found with this tag" 
                : `${notesWithContext.length} note${notesWithContext.length === 1 ? '' : 's'} found`
              }
            </p>
          </div>
        </div>
      </div>

      {notesWithContext.length === 0 ? (
        <div className="text-center py-16">
          <div className="bg-white/5 backdrop-blur-sm rounded-lg p-12 border border-white/10 max-w-md mx-auto">
            <Tag className="h-16 w-16 text-purple-400 mx-auto mb-6" />
            <h3 className="text-2xl font-bold text-white mb-4">No Notes Found</h3>
            <p className="text-purple-200 mb-6">
              No accessible notes were found with the tag "{selectedTag}". This could mean:
            </p>
            <ul className="text-purple-200 text-sm space-y-2 text-left">
              <li>• No notes have been tagged with "{selectedTag}"</li>
              <li>• You don't have access to notes with this tag</li>
              <li>• The notes may be in campaigns you're no longer part of</li>
            </ul>
          </div>
        </div>
      ) : (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {notesWithContext.map(({ note, session, campaign }) => (
            <Card 
              key={note.id} 
              className="bg-white/10 backdrop-blur-sm border-white/20 hover:bg-white/15 transition-all cursor-pointer group"
              onClick={() => handleNoteClick({ note, session, campaign })}
            >
              <CardHeader className="pb-3">
                <div className="flex items-start justify-between">
                  <CardTitle className="text-white text-lg group-hover:text-purple-300 transition-colors">
                    {note.name}
                  </CardTitle>
                  <FileText className="h-5 w-5 text-purple-400 flex-shrink-0" />
                </div>
                <div className="space-y-2">
                  <div className="flex items-center text-sm text-purple-200">
                    <MapPin className="h-4 w-4 mr-1" />
                    <span className="truncate">{campaign.name}</span>
                  </div>
                  <div className="flex items-center text-sm text-purple-200">
                    <Calendar className="h-4 w-4 mr-1" />
                    <span className="truncate">{session.name}</span>
                  </div>
                </div>
              </CardHeader>
              <CardContent className="pt-0">
                <div className="space-y-3">
                  {note.inGameDateTime && (
                    <div className="text-sm text-purple-300">
                      <span className="font-medium">Date:</span> {note.inGameDateTime}
                    </div>
                  )}
                  
                  {note.tags.length > 0 && (
                    <div className="flex flex-wrap gap-1">
                      {note.tags.map((tag, index) => (
                        <Badge 
                          key={index} 
                          variant={tag === selectedTag ? "default" : "secondary"}
                          className={tag === selectedTag 
                            ? "bg-purple-600 text-white" 
                            : "bg-white/20 text-purple-200 hover:bg-white/30"
                          }
                        >
                          {tag}
                        </Badge>
                      ))}
                    </div>
                  )}
                  
                  <div className="text-xs text-purple-300 pt-2 border-t border-white/10">
                    Click to view note location
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
