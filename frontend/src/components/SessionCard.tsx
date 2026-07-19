import React, { useState, useEffect } from 'react';
import { Session, Participant, SessionPlayer } from '../backend';
import { useGetSessionPlayers, useGetUserProfile } from '../hooks/useQueries';
import { useInternetIdentity } from '../hooks/useInternetIdentity';
import SessionPlayersDialog from './SessionPlayersDialog';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Clock, Calendar, User, Loader2, Users, Eye } from 'lucide-react';

interface SessionCardProps {
  session: Session;
  participants: Array<Participant & { name?: string }>;
  onSessionUpdated?: () => void;
  onSessionClick?: (session: Session) => void;
}

interface SessionPlayerWithProfile extends SessionPlayer {
  name?: string;
}

export default function SessionCard({ session, participants, onSessionUpdated, onSessionClick }: SessionCardProps) {
  const [isSessionPlayersDialogOpen, setIsSessionPlayersDialogOpen] = useState(false);
  const [sessionPlayers, setSessionPlayers] = useState<SessionPlayerWithProfile[]>([]);
  const [loadingSessionPlayers, setLoadingSessionPlayers] = useState(false);
  
  const getSessionPlayers = useGetSessionPlayers();
  const getUserProfile = useGetUserProfile();
  const { identity } = useInternetIdentity();

  const currentUserPrincipal = identity?.getPrincipal();
  const isSessionOwner = session.creator.toString() === currentUserPrincipal?.toString();

  // Find the creator's name
  const creator = participants.find(p => p.principal.toString() === session.creator.toString());
  const creatorName = creator?.name || 'Unknown GM';

  useEffect(() => {
    loadSessionPlayers();
  }, [session.id]);

  const loadSessionPlayers = async () => {
    setLoadingSessionPlayers(true);
    try {
      const playerList = await getSessionPlayers.mutateAsync(session.id);
      const playersWithProfiles = await Promise.all(
        playerList.map(async (player) => {
          try {
            const profile = await getUserProfile.mutateAsync(player.participant);
            return {
              ...player,
              name: profile?.name,
            };
          } catch {
            return player;
          }
        })
      );
      setSessionPlayers(playersWithProfiles);
    } catch (error) {
      console.error('Failed to load session players:', error);
      setSessionPlayers([]);
    } finally {
      setLoadingSessionPlayers(false);
    }
  };

  // Format the date
  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    } catch {
      return dateString;
    }
  };

  const handleSessionPlayersUpdated = () => {
    loadSessionPlayers();
  };

  const handleCardClick = (e: React.MouseEvent) => {
    // Don't trigger card click if clicking on buttons
    if ((e.target as HTMLElement).closest('button')) {
      return;
    }
    onSessionClick?.(session);
  };

  return (
    <>
      <Card 
        className="bg-white/5 backdrop-blur-sm border-white/10 hover:bg-white/10 transition-all duration-200 group cursor-pointer"
        onClick={handleCardClick}
      >
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between">
            <div className="flex items-center space-x-2 flex-1 min-w-0">
              <Clock className="h-5 w-5 text-blue-400 flex-shrink-0" />
              <div className="min-w-0 flex-1">
                <CardTitle className="text-white text-lg truncate">{session.name}</CardTitle>
                <div className="flex items-center space-x-4 mt-1">
                  <div className="flex items-center space-x-1 text-sm text-purple-300">
                    <Calendar className="h-4 w-4" />
                    <span>{formatDate(session.date)}</span>
                  </div>
                  <div className="flex items-center space-x-1 text-sm text-purple-300">
                    <User className="h-4 w-4" />
                    <span>by {creatorName}</span>
                    {isSessionOwner && (
                      <span className="text-xs text-amber-400 ml-1">(You)</span>
                    )}
                  </div>
                </div>
              </div>
            </div>
            <div className="flex items-center space-x-2 flex-shrink-0">
              <Badge variant="outline" className="text-xs text-blue-300 border-blue-400/30">
                Session
              </Badge>
              <Button
                onClick={() => setIsSessionPlayersDialogOpen(true)}
                variant="ghost"
                size="sm"
                className="h-8 px-2 text-purple-300 hover:text-white hover:bg-white/10 opacity-0 group-hover:opacity-100 transition-opacity"
              >
                {isSessionOwner ? <Users className="h-4 w-4 mr-1" /> : <Eye className="h-4 w-4 mr-1" />}
                <span className="text-xs">{isSessionOwner ? 'Manage' : 'View'}</span>
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-3">
          {session.description && (
            <CardDescription className="text-purple-200 whitespace-pre-wrap">
              {session.description}
            </CardDescription>
          )}
          
          {/* Session Players Summary */}
          <div className="flex items-center justify-between pt-2 border-t border-white/10">
            <div className="flex items-center space-x-2">
              <Users className="h-4 w-4 text-blue-400" />
              <span className="text-sm text-purple-200">
                {loadingSessionPlayers ? (
                  <Loader2 className="h-3 w-3 animate-spin inline" />
                ) : (
                  `${sessionPlayers.length} player${sessionPlayers.length === 1 ? '' : 's'}`
                )}
              </span>
            </div>
            {sessionPlayers.length > 0 && !loadingSessionPlayers && (
              <div className="flex items-center space-x-1">
                {sessionPlayers.slice(0, 3).map((player, index) => (
                  <span key={player.participant.toString()} className="text-xs text-purple-300">
                    {player.name || 'Unknown'}
                    {index < Math.min(sessionPlayers.length, 3) - 1 && ', '}
                  </span>
                ))}
                {sessionPlayers.length > 3 && (
                  <span className="text-xs text-purple-300">+{sessionPlayers.length - 3} more</span>
                )}
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Session Players Dialog */}
      <SessionPlayersDialog
        session={session}
        campaignParticipants={participants}
        open={isSessionPlayersDialogOpen}
        onOpenChange={setIsSessionPlayersDialogOpen}
        onSessionPlayersUpdated={handleSessionPlayersUpdated}
      />
    </>
  );
}
